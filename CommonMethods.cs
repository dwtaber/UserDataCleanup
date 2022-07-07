namespace UserDataCleanup;

public class CommonMethods
{
    public static bool TestElevated()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static bool TestUserContextType(ContextType contextType, out SecurityIdentifier sid)
    {
        var context = new PrincipalContext(contextType);
        sid = Principal.FindByIdentity(context, Environment.UserName).Sid;
        return null != sid;
    }

    public static SecurityIdentifier GetComputerJoinedDomainSid()
    {
        var context = new PrincipalContext(ContextType.Domain);
        return Principal.FindByIdentity(context, Environment.MachineName).Sid;
    }

    public static bool TryGetComputerJoinedDomainSid(out SecurityIdentifier sid)
    {
        sid = GetComputerJoinedDomainSid();
        return sid != null;
    }

    public static string GetComputerDomainNameNoTld()
    {
        return Domain.GetComputerDomain().Name.Split('.')[0];
    }

    public static SecurityIdentifier GetLocalMachineSid()
    {
        return new NTAccount("DefaultAccount").ToSecurityIdentifier().AccountDomainSid!;
    }
}
