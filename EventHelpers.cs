namespace UserDataCleanup;

public class EventHelpers
{
    public static List<LogOnRecord> GetRecentLogons(TimeSpan timeSpan, bool includeSystemLogons = false, bool includeServiceLogons = false)
    {
        var milliseconds = timeSpan.TotalMilliseconds;
        var systemSelector = $"*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= {milliseconds}]]]";
        // var excludeSystemLogons = "*[EventData[ (Data[@Name='LogonType']=5) ]]";
        var eventDataSelector = new XPathSelector("EventData", XPathBooleanOperator.and);
        var logonType = "LogonType";
        var excludeSystem = new XPathDataTerm(logonType, XPathComparisonOperator.ne, (int)LogonType.System);
        var excludeService = new XPathDataTerm(logonType, XPathComparisonOperator.ne, (int)LogonType.Service);
        var termGroup = new XPathTermGroup();
        if (!includeSystemLogons) { termGroup.XPathTerms.Add(excludeSystem); }
        if (!includeServiceLogons) { termGroup.XPathTerms.Add(excludeService); }
        eventDataSelector.XPathTermGroups.Add(termGroup);
        var xPath = string.Join(" and ", systemSelector, eventDataSelector.ToString());


        var query = new EventLogQuery("Security", PathType.LogName, xPath) { ReverseDirection = true };
        var allResults = new List<LogOnRecord>();
        using (var reader = new EventLogReader(query))
        {
            for (var record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
            {
                allResults.Add(new LogOnRecord((EventLogRecord)record));
            }
        }
        return allResults.GroupBy(x => x.TargetUserSid)
                         .Select(group => group.First())
                         .ToList();
    }

    public static List<LogOnRecord> GetRecentDomainLogons(TimeSpan timeSpan)
    {
        return GetRecentLogons(timeSpan)
            .Where(x => x.TargetUserSid!.AccountDomainSid != null)
            .ToList();
    }
}
