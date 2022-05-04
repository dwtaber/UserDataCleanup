namespace UserDataCleanup;

public class XPathDataTerm
{
    public string Name { get; set; }
    public object? Value { get; set; }

    public override string ToString()
    {
        return $"Data[@Name='{Name}']={Value}";
    }

    public XPathDataTerm(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }
}