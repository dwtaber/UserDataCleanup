namespace UserDataCleanup;

public static class RegistryKeyExtension
{
    public static bool TryOpenSubKey(this RegistryKey key,
                                     string name,
                                     [NotNullWhen(true)] out RegistryKey? subKey)
    {
        subKey = key.OpenSubKey(name);
        return subKey != null;
    }
}
