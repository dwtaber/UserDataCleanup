namespace UserDataCleanup;

public class CommonMethods
{
    public static bool TestElevated()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static SecurityIdentifier GetComputerJoinedDomainSid()
    {
        var adsi = new DirectoryEntry($"LDAP://{Domain.GetComputerDomain()}");
        var bytes = adsi.Properties["ObjectSid"].Value as byte[];
        return bytes != null ? new SecurityIdentifier(bytes, 0) : throw new Exception();
    }

    public static string GetComputerDomainNameNoTld()
    {
        return Domain.GetComputerDomain().Name.Split('.')[0];
    }

    public static SecurityIdentifier GetLocalMachineSid()
    {
        return new NTAccount("DefaultAccount").ToSecurityIdentifier().AccountDomainSid!;
    }
}
