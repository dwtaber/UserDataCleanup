namespace UserDataCleanup;
public static class NTAccountExtension
{
    public static SecurityIdentifier ToSecurityIdentifier(this NTAccount nta)
    {
        return (SecurityIdentifier)nta.Translate(typeof(SecurityIdentifier));
    }

    public static bool TryTranslate(this NTAccount nta, out SecurityIdentifier? sid)
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