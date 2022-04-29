namespace UserDataCleanup;

public class UserProfileInfo
{
    public string KeyName { get; }
    public string RelativeKeyName { get; }
    public SecurityIdentifier? Sid { get; }
    public string? ProfilePath { get; }
    public bool ProfilePathExists => Directory.Exists(ProfilePath);
    public bool PathIsStandard { get; }
    public string? DomainName { get; }
    public string? UserName { get; }
    public bool IsAccountSid { get; }
    public DateTime? LastSignOn { get; internal set; }
    public bool IsEqualDomainSid(SecurityIdentifier domainSid)
    {
        return Sid != null ? Sid.IsEqualDomainSid(domainSid) : false;
    }
    public bool IsInComputerdomain()
    {
        return IsEqualDomainSid(CommonMethods.GetComputerDomainSid());
    }

    public UserProfileInfo(RegistryKey regKey)
    {
        KeyName = regKey.Name;
        RelativeKeyName = Path.GetRelativePath(RegistryHelpers.HKLM, KeyName);
        RegistryHelpers.TryGetProfileSid(regKey, out var sid);
        Sid = sid;
        ProfilePath = (string?)regKey.GetValue("ProfileImagePath");
        IsAccountSid = Sid != null ? Sid.IsAccountSid() : false;
        if (Sid != null && Sid.TryTranslate(out var nta))
        {
            var splitQualifiedName = nta.Value.Split(@"\");
            DomainName = splitQualifiedName[0];
            UserName = splitQualifiedName[1];
        }
        else
        {
            DomainName = null;
            UserName = null;
        }
        PathIsStandard = string.Equals(ProfilePath,
                                       Path.Join(RegistryHelpers.ProfilesDirectory, UserName),
                                       StringComparison.OrdinalIgnoreCase);
    }

    public static List<UserProfileInfo> GetAll()
    {
        var list = new List<UserProfileInfo>();
        foreach (var name in RegistryHelpers.ProfileList.GetSubKeyNames())
        {
            var key = RegistryHelpers.ProfileList.OpenSubKey(name);
            if (key != null)
            {
                list.Add(new UserProfileInfo(key));
            }
        }
        return list;
    }

    public static List<UserProfileInfo> GetAllUsers()
    {
        return GetAll().Where(x => x.IsAccountSid).ToList();
    }
}
