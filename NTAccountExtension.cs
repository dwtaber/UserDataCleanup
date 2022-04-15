namespace UserDataCleanup;
public static class NTAccountExtension
{
    public static SecurityIdentifier ToSecurityIdentifier(this NTAccount nta)
    {
        return nta.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
    }

    public static bool TryTranslate(this NTAccount nta, out SecurityIdentifier sid)
    {
        try
        {
            sid = nta.ToSecurityIdentifier();
            return true;
        }
        catch (IdentityNotMappedException)
        {
            sid = null;
            return false;
        }
    }
}