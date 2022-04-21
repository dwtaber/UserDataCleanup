namespace UserDataCleanup;

public class FilesystemMethods
{
    public static string DomainName { get; } = CommonMethods.GetComputerDomainNameNoTld();
    public static SecurityIdentifier DomainSid { get; } = CommonMethods.GetComputerDomainSid();

}
