namespace UserDataCleanup;

public class XPathDataTerm
{
    public string Name { get; set; }
    public object? Value { get; set; }
    public string XPathComparisonOperator
    {
        get => XPathComparisonOperatorHelper.Conversions[_xPathComparisonOperator];
    }

    private XPathComparisonOperator _xPathComparisonOperator;

    public override string ToString()
    {
        return $"Data[@Name='{Name}']{XPathComparisonOperator}{Value}";
    }

    public XPathDataTerm(string name, XPathComparisonOperator comparisonOperator, object? value = null)
    {
        Name = name;
        _xPathComparisonOperator = comparisonOperator;
        Value = value;
    }
}