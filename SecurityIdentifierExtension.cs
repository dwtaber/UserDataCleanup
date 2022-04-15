namespace UserDataCleanup;
public static class SecurityIdentifierExtension
{
    public static NTAccount ToNTAccount(this SecurityIdentifier sid)
    {
        return sid.Translate(typeof(NTAccount)) as NTAccount;
    }

    public static bool TryTranslate(this SecurityIdentifier sid, out NTAccount nta)
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
}