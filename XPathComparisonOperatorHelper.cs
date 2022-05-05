namespace UserDataCleanup;

public class XPathComparisonOperatorHelper
{
    public static Dictionary<XPathComparisonOperator, string> Conversions { get; } = new Dictionary<XPathComparisonOperator, string>()
    {
        { XPathComparisonOperator.eq, "=" },
        { XPathComparisonOperator.gt, ">" },
        { XPathComparisonOperator.lt, "<" },
        { XPathComparisonOperator.ne, "!=" },
        { XPathComparisonOperator.ge, ">=" },
        { XPathComparisonOperator.le, "<=" },
    };
}
