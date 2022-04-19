namespace UserDataCleanup;

internal class CommonMethods
{
    public static bool TestElevated()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
