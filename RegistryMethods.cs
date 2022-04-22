namespace UserDataCleanup;

public class RegistryMethods
{
    public const string ProfileListPath =
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

    public static string GetProfilesDirectory()
    {
        var value = Registry.GetValue(RegistryMethods.ProfileListPath, "ProfilesDirectory", null);
        return value != null ? (string)value : throw new Exception();
    }
}
