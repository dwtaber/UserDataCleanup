using namespace System.Collections.Generic
using namespace System.Diagnostics.Eventing.Reader
using namespace System.Management.Automation.Runspaces
using namespace System.Security.Principal
using namespace Microsoft.Win32

$DomainSid = (NameToSid "$($env:COMPUTERNAME)$").AccountDomainSid
$RegParentPath = "Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList"

function NameToSid ([NTAccount]$UserName)
{
    return $UserName.Translate([SecurityIdentifier])
}

function SidToName ([SecurityIdentifier]$Sid)
{
    return $Sid.Translate([NTAccount]) | Split-Path -Leaf
}

function BytesToSid ([byte[]]$Bytes)
{
    return [SecurityIdentifier]::new($Bytes, 0)
}

function BytesToName ([byte[]]$Bytes)
{
    return SidToName (BytesToSid $Bytes)
}

function GetSignedInUsers
{
    quser 2>$null |
    ForEach-Object { ($_ -replace '>','') -replace '\s{2,}',',' } |
    ConvertFrom-Csv |
    Select-Object -ExpandProperty Username
}

class SignOnRecord
{
    [string]$Username
    [SecurityIdentifier]$Sid
    [datetime]$Timestamp

    SignOnRecord ([EventLogRecord]$EventLogRecord)
    {
        $this.Username = $EventLogRecord.Properties[5].Value
        $this.Sid = $EventLogRecord.Properties[4].Value
        $this.Timestamp = $EventLogRecord.TimeCreated
    }
}

function GetLastSignOnPerUser ([double]$MaxDays)
{
    $Milliseconds = [timespan]::FromDays($MaxDays).TotalMilliseconds
    $XPath = "*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= $Milliseconds]]]"
    Get-WinEvent -LogName "Security" -FilterXPath $XPath |
        ForEach-Object { [SignOnRecord]::New($_) } |
        Where-Object {$_.sid.IsEqualDomainSid($DomainSid)} |
        Sort-Object -Property @{e="Username";Descending=$false},@{e="Timestamp";descending=$true} |
        Group-Object -Property username |
        Select-Object group |
        ForEach-Object {$_.group[0]}
}

function ConvertToUsernameSidDict ([SignOnRecord[]]$Records)
{
    $dict = [Dictionary[[string],[SecurityIdentifier]]]::New()
    foreach ($record in $Records)
    {
        $dict.Add($record.Username, $record.Sid)
    }
    return $dict
}

function ParseKeyName ([RegistryKey]$Key)
{
    [SecurityIdentifier]$Key.PSChildName.Replace(".bak","")
}
function GetDomainUserRegKeys
{
    Get-ChildItem $RegParentPath |
        Where-Object {(ParseKeyName $_).IsEqualDomainSid($DomainSid) -eq $true} |
        Where-Object {(ParseKeyName $_).IsWellKnown([WellKnownSidType]::AccountAdministratorSid) -eq $false}
}

function RemoveUserFolder ([string]$Path)
{    
    $OneDriveCommercialExists = Test-Path "$Path\OneDrive - *"
    
    # Workaround for bug that prevents removing reparse points created by OneDrive.
    if ($OneDriveCommercialExists)
    {
        Write-Information -MessageData "OneDrive reparse point in $Path; deleting folder via cmd.exe"
        cmd /c rmdir "$Path" /q /s
    }
    else
    {
        # Deleting files with this cmdlet when possible to avoid losing error details.
        Remove-Item -Path $Path -Recurse -Force
    }
}

function RemoveKeyAndFolder ([RegistryKey]$Key)
{
    if ($Key.PSParentPath -ne $RegParentPath)
    {
        throw "Not a subkey of ProfileList."
    }
    $FolderPath = $Key.GetValue("ProfileImagePath")
    if ($null -ne $FolderPath)
    {
        $FolderExists = Test-Path $FolderPath
    }
    else
    {
        $FolderExists = $false
    }
    if ($FolderExists)
    {
        RemoveUserFolder -Path $FolderPath
    }
    $Key | Remove-Item -Force -Recurse
}

function Remove-OldUserProfile
{
    [CmdletBinding(DefaultParameterSetName="FromDays")]
    param
    (
        [Parameter(
            ParameterSetName = "FromDays",
            Mandatory = $true
        )]
        [Alias("Days","DaysOld")]
        [double]
        $FromDays,

        [Parameter(
            ParameterSetName = "FromDate",
            Mandatory = $true
        )]
        [datetime]
        $FromDate,

        [string[]]
        $Exclude,

        # Removes backup keys and keys with temp profile folders, even if they've been used recently.
        # Items excluded by parameter are still skipped, as are the current user and any signed-in users.
        [switch]
        $Force
    )

    begin
    {
        if ($PSCmdlet.ParameterSetName -eq "FromDate")
        {
            $FromDays = ([datetime]::Today - $FromDate).TotalDays
        }
        elseif ($PSCmdlet.ParameterSetName -eq "FromDays")
        {
            $FromDate = [datetime]::Today - [timespan]::FromDays($FromDays)
        }

        $CmdletUserSid = [WindowsIdentity]::GetCurrent().User

        Write-Information -MessageData "Querying Security log for recent sign-on events."
        $RecentUsers = GetLastSignOnPerUser $FromDays
        $RecentUsersDict = ConvertToUsernameSidDict $RecentUsers

        Write-Information -MessageData "Checking for signed-in users."
        $SignedInUsers = GetSignedInUsers

        Write-Information -MessageData "Getting Registry keys for domain users."
        $DomainUserRegKeys = GetDomainUserRegKeys
    }

    process
    {
        foreach ($Key in $DomainUserRegKeys)
        {
            $KeyName = $Key.PSChildName
            $Sid = ParseKeyName $Key
            $FolderPath = $Key.GetValue("ProfileImagePath")
            if ($null -ne $FolderPath)
            {
                $FolderName = $FolderPath | Split-Path -Leaf
            }
            else 
            {
                $FolderName = $null
            }
            $IsExcluded = ($null -ne $Exclude) -and
            (
                ($Sid -in $Exclude) -or
                ($KeyName -in $Exclude) -or
                ($FolderName -in $Exclude) -or
                ($FolderPath -in $Exclude)
            )
            $IsCmdletUser = $Sid -eq $CmdletUserSid
            $IsSignedIn = $RecentUsersDict[$Sid] -in $SignedInUsers
            $ProfilePathExists = Test-Path -Path $FolderPath
            $IsProfilePathNull = $null -eq $FolderPath
            $IsTempFolder = $FolderName -like "TEMP.*"
            $IsBackupKey = $KeyName -like "*.bak"
            $IsRecent = $Sid -in $RecentUsers.Sid
            $ShouldRemove = ( $IsCmdletUser -or $IsSignedIn -or $IsRecent -or $IsExcluded ) -eq $false

            switch ($true)
            {
                # Don't mess with the account running the command, regardless of other considerations.
                ( $IsCmdletUser )
                {
                    Write-Information -MessageData "$Sid is the user executing this command. Skipping" 
                    break 
                }

                # Don't mess with any signed-in users, regardless of other considerations.
                ( $IsSignedIn )
                {
                    Write-Information -MessageData "$($RecentUsersDict[$Sid]) is currently signed in. Skipping" 
                    break 
                }

                # Items excluded by the -Exclude parameter are always skipped, even with the -Force switch.
                ( $IsExcluded )
                {
                    Write-Information -MessageData "$KeyName is excluded. Skipping."
                    break
                }

                # Remove incomplete keys, even if they've been used recently.
                ( $IsProfilePathNull )
                {
                    Write-Information -MessageData "$KeyName contains no profile path. Removing."
                    RemoveKeyAndFolder $Key
                    break
                }

                # Remove keys where the profile folder doesn't exist, even if they've been used recently.
                ( $ProfilePathExists -eq $false)
                {
                    Write-Information -MessageData "$KeyName has no profile directory. Removing."
                    RemoveKeyAndFolder $Key
                    break
                }

                # With the -Force switch, keys with temp folders get removed, even if they've been used recently.
                ( $IsTempFolder -and $Force )
                {
                    Write-Information -MessageData "$FolderName is a temp folder and the Force switch was used. Removing."
                    RemoveKeyAndFolder $Key
                    break
                }

                # With the -Force switch, backup keys get removed, even if they've been used recently.
                ( $IsBackupKey -and $Force )
                {
                    Write-Information -MessageData "$KeyName is a backup key and the Force switch was used. Removing"
                    RemoveKeyAndFolder $Key
                    break
                }

                ( $IsRecent )
                {
                    Write-Information -MessageData "$KeyName has been used recently. Skipping."
                    break
                }

                ( $ShouldRemove )
                {
                    Write-Information -MessageData "Removing $Keyname"
                    RemoveKeyAndFolder $Key
                    break
                }

                # This shouldn't happen
                default
                {
                    Write-Information -MessageData "UNEXPECTED: No cases apply to $KeyName. Skipping"
                }
            }
        }
    }

    end
    {
        # Includes all remaining profiles, not just domain users, just in case.
        $RemainingKeyAssociatedFolders = Get-ChildItem -Path $RegParentPath |
            ForEach-Object {$_.GetValue("ProfileImagePath")} |
            Sort-Object

        $RemainingTempFolders = Get-ChildItem -Path "C:\Users" -Filter "TEMP.*" |
            Select-Object -ExpandProperty FullName
        
        $OrphanedTempFolders = $RemainingTempFolders |
            Where-Object {$_ -notin $RemainingKeyAssociatedFolders}

        # Remove any temporary profile folders not associated with a remaining profile.
        foreach ($Folder in $OrphanedTempFolders)
        {
            Write-Information -MessageData "Removing orphaned folder $Folder"
            RemoveUserFolder $Folder
        }

    }
}
