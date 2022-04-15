namespace UserDataCleanup;

public class UserIDCombo
{
    public string DomainName { get; }
    public string UserName { get; }
    public NTAccount NTAccount { get; }
    public SecurityIdentifier SecurityIdentifier { get; }

    public UserIDCombo(NTAccount ntAccount) : this(ntAccount.ToSecurityIdentifier())
    {}

    public UserIDCombo (SecurityIdentifier sid)
    {
        SecurityIdentifier = sid;
        NTAccount = SecurityIdentifier.ToNTAccount();
        var split = NTAccount.Value.Split(@"\");
        DomainName = split[0];
        UserName = split[1];
    }
}
