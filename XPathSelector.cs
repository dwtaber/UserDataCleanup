namespace UserDataCleanup;
public class XPathSelector : XPathTermGroup
{
    public string Node { get; set; }

    public XPathSelector( string node, XPathBooleanOperator booleanOperator)
    {
        Node = node;
        XPathBooleanOperator = booleanOperator;
    }
    public override string ToString()
    {
        var hasLooseTerms = XPathTerms.Count != 0;
        var hasGroups = XPathTermGroups.Count != 0;
        var prefix = $"*[{Node}[";
        var postfix = "]]";

        return (hasLooseTerms, hasGroups) switch
        {
            (true, false)  => $"{prefix}{XPathTermsString!}{postfix}",
            (false, true)  => $"{prefix}{XPathTermGroupsString!}{postfix}",
            (true, true)   => $"{prefix}{string.Join(separator, XPathTermsString, XPathTermGroupsString)}{postfix}",
            (false, false) => string.Empty
        };
    }
}
