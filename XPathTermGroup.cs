namespace UserDataCleanup;

public class XPathTermGroup
{
    public List<XPathDataTerm> XPathTerms { get; set; } = new List<XPathDataTerm>();
    public List<XPathTermGroup> XPathTermGroups { get; set; } = new List<XPathTermGroup>();
    public XPathBooleanOperator XPathBooleanOperator { get; set; }
    internal string Separator => $" {XPathBooleanOperator} ";
    public string? XPathTermsString
    {
        get => XPathTerms.Count != 0 ? $"{ string.Join(Separator, XPathTerms.Select(x => x.ToString()))}" : null;
    }
    public string? XPathTermGroupsString
    {
        get => XPathTermGroups.Count != 0 ? $"{ string.Join(Separator, XPathTermGroups.Select(x => x.ToString()))}" : null;
    }
    public override string ToString()
    {        
        var hasLooseTerms = XPathTerms.Count != 0;
        var hasGroups = XPathTermGroups.Count != 0;

        return (hasLooseTerms, hasGroups) switch
        {
            (true, false) => $"({XPathTermsString!})",
            (false, true) => $"({XPathTermGroupsString!})",
            (true, true) => $"({string.Join(Separator, XPathTermsString, XPathTermGroupsString)})",
            (false, false) => string.Empty
        };
    }
}
