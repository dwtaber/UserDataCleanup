namespace UserDataCleanup;
public static class SecurityIdentifierExtension
{
    public static NTAccount ToNTAccount(this SecurityIdentifier sid)
    {
        return (NTAccount)sid.Translate(typeof(NTAccount));
    }

    public static bool TryTranslate(this SecurityIdentifier sid, [NotNullWhen(true)] out NTAccount? nta)
    {
        try
        {
            nta = sid.ToNTAccount();
            return true;
        }
        catch (IdentityNotMappedException)
        {
            nta = null;
            return false;
        }
    }

    public static string[] ToStringArray(this SecurityIdentifier sid) => sid.Value.Split("-");

    public static bool IsDomainSubauthority(this SecurityIdentifier sid)
    {
        var split = sid.ToStringArray();
        var isNTAuth = split[2] == "5";
        var isNTDomain = split[3] == "21";
        return isNTAuth && isNTDomain;
    }

    public static bool IsComputerJoinedDomain(this SecurityIdentifier sid) => sid.IsEqualDomainSid(CommonMethods.GetComputerJoinedDomainSid());

    public static bool IsLocalMachineDomain(this SecurityIdentifier sid) => sid.IsEqualDomainSid(CommonMethods.GetLocalMachineSid());
}