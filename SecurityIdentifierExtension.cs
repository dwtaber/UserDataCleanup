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
}