namespace UserDataCleanup;

internal class CommonMethods
{
    public static bool TestElevated()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public SecurityIdentifier GetComputerDomainSid()
    {
        var adsi = new DirectoryEntry($"LDAP://{Domain.GetComputerDomain()}");
        var bytes = adsi.Properties["ObjectSid"].Value as byte[];
        return new SecurityIdentifier(bytes, 0);
    }

    public string GetComputerDomainNameNoTld()
    {
        return Domain.GetComputerDomain().Name.Split('.')[0];
    }
}
