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

    public static bool IsDomainSubauthority(this SecurityIdentifier sid)
    {
        var split = sid.Value.Split("-");
        var isNTAuth = split[2] == "5";
        var isNTDomain = split[3] == "21";
        return isNTAuth && isNTDomain;
    }
}