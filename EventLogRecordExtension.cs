namespace UserDataCleanup;

public static class EventLogRecordExtension
{
    public static object GetPropertyValue(this EventLogRecord record, string propertyName)
    {
        var xPath = new List<string> {$"Event/EventData/Data[@Name='{propertyName}']"};
        var selector = new EventLogPropertySelector(xPath);
        return record.GetPropertyValues(selector)[0];
    }
}
