namespace UserDataCleanup;

public class RegistryMethods
{
    public const string ProfileListSubkey =
        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

    public static RegistryKey ProfileList = Registry.LocalMachine.OpenSubKey(ProfileListSubkey)!;

    public static string GetProfilesDirectory()
    {
        var value = ProfileList.GetValue("ProfilesDirectory");
        return value != null ? (string)value : throw new Exception();
    }

    public static SecurityIdentifier GetProfileSid(RegistryKey ProfileKey)
    {
        var bytes = (byte[]?)ProfileKey.GetValue("Sid");
        if (bytes != null)
        {
            return new SecurityIdentifier(bytes, 0);
        }
        else
        {
            throw new Exception();
        }
    }

    public static SecurityIdentifier GetProfileSid(string name)
    {
        if (ProfileList.TryOpenSubKey(name, out var subKey))
        {
            var bytes = (byte[])subKey.GetValue("Sid");
            return new SecurityIdentifier(bytes, 0);
        }
        else
        {
            throw new ItemNotFoundException($"Could not open subkey '{name}'");
        }
    }

    public static bool TryGetProfileSid(RegistryKey ProfileKey,
                                        [NotNullWhen(true)] out SecurityIdentifier? profileSid)
    {
        var bytes = (byte[]?)ProfileKey.GetValue("Sid");
        if (bytes != null)
        {
            profileSid = new SecurityIdentifier(bytes, 0);
            return true;
        }
        else
        {
            profileSid = null;
            return false;
        }
    }

    public static bool TryGetProfileSid(string name,
                                        [NotNullWhen(true)] out SecurityIdentifier? profileSid)
    {
        if (ProfileList.TryOpenSubKey(name, out var subKey))
        {
            return TryGetProfileSid(subKey, out profileSid);
        }
        else
        {
            throw new ItemNotFoundException($"Could not open subkey '{name}'");
        }
    }
}
