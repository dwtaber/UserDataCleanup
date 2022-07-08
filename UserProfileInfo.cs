namespace UserDataCleanup;

public class UserProfileInfo
{
    public string KeyName { get; }
    public string RelativeKeyName { get; }
    public SecurityIdentifier? Sid { get; }
    public DirectoryInfo? ProfileDirectory { get; }
    public bool ProfilePathExists => ProfileDirectory?.Exists ?? false;
    public bool ProfileIsInProfilesDirectory
    {
        get
        {
            var parent = ProfileDirectory?.Parent?.FullName;
            var profiles = RegistryHelpers.ProfilesDirectory.FullName;
            return string.Equals(parent, profiles, StringComparison.OrdinalIgnoreCase);
        }
    }
    public bool PathIsStandard => UserName != null
                               && ProfileDirectory != null
                               && string.Equals(ProfileDirectory?.Name, UserName, StringComparison.OrdinalIgnoreCase);
    public string? DomainName { get; }
    public string? UserName { get; }
    public bool IsAccountSid { get; }
    public DateTime? LastSignOn { get; internal set; }

    public bool IsInComputerJoinedDomain()
    {
        return Sid != null && Sid.IsComputerJoinedDomain();
    }
    public bool IsInLocalMachineDomain()
    {
        return Sid != null && Sid.IsLocalMachineDomain();
    }

    public UserProfileInfo(RegistryKey regKey)
    {
        KeyName = regKey.Name;
        RelativeKeyName = Path.GetRelativePath(RegistryHelpers.HKLM, KeyName);
        RegistryHelpers.TryGetProfileSid(regKey, out var sid);
        Sid = sid;
        var profilePath = (string?)regKey.GetValue("ProfileImagePath");

        IsAccountSid = Sid != null && Sid.IsAccountSid();
        if (Sid != null && Sid.TryTranslate(out var nta))
        {
            var splitQualifiedName = nta.Value.Split(@"\");
            DomainName = splitQualifiedName[0];
            UserName = splitQualifiedName[1];
        }
        else
        {
            DomainName = null;
            UserName = null;
        }
    }

    public static List<UserProfileInfo> GetAll()
    {
        var list = new List<UserProfileInfo>();
        foreach (var name in RegistryHelpers.ProfileList.GetSubKeyNames())
        {
            var key = RegistryHelpers.ProfileList.OpenSubKey(name);
            if (key != null)
            {
                list.Add(new UserProfileInfo(key));
            }
        }
        return list;
    }

    public static List<UserProfileInfo> GetAllUsers()
    {
        return GetAll().Where(x => x.IsAccountSid).ToList();
    }
}
