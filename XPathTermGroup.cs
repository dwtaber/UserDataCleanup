namespace UserDataCleanup;

public class XPathTermGroup
{
    public List<XPathDataTerm>? XPathTerms { get; set; }
    public List<XPathTermGroup>? XPathTermGroups { get; set; }
    public XPathBooleanOperator XPathBooleanOperator { get; set; }
    internal string separator => $" {XPathBooleanOperator} ";
    public string? XPathTermsString => XPathTerms != null ? $"{ string.Join(separator, XPathTerms.Select(x => x.ToString()))}" : null;
    public string? XPathTermGroupsString => XPathTermGroups != null ? $"{ string.Join(separator, XPathTermGroups.Select(x => x.ToString()))}" : null;
    public override string ToString()
    {        
        var hasLooseTerms = string.IsNullOrEmpty(XPathTermsString);
        var hasGroups = string.IsNullOrEmpty(XPathTermGroupsString);

        return (hasLooseTerms, hasGroups) switch
        {
            (true, false) => XPathTermsString!,
            (false, true) => XPathTermGroupsString!,
            (true, true) => $"({string.Join(separator, XPathTermsString, XPathTermGroupsString)})",
            (false, false) => ""
        };
    }
}
