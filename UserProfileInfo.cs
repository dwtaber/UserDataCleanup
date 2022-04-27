namespace UserDataCleanup;

public class UserProfileInfo
{
    public string KeyName { get; }
    public SecurityIdentifier? Sid { get; }
    public string? ProfilePath { get; }
    public string? DomainName { get; }
    public string? UserName { get; }
    public bool IsAccountSid { get; }
    public DateTime? LastSignOn { get; internal set; }
    public bool IsEqualDomainSid(SecurityIdentifier domainSid)
    {
        return Sid != null ? Sid.IsEqualDomainSid(domainSid) : false;
    }
    public bool IsInComputerdomain()
    {
        return IsEqualDomainSid(CommonMethods.GetComputerDomainSid());
    }

    public UserProfileInfo(RegistryKey regKey)
    {
        KeyName = regKey.Name;
        RegistryMethods.TryGetProfileSid(regKey, out var sid);
        Sid = sid;
        ProfilePath = (string?)regKey.GetValue("ProfileImagePath");
        IsAccountSid = Sid != null ? Sid.IsAccountSid() : false;
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
}
