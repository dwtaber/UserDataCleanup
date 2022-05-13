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
        if (!includeSystemLogons) { eventDataSelector.XPathTerms.Add(excludeSystem); }
        if (!includeServiceLogons) { eventDataSelector.XPathTerms.Add(excludeService); }
        var xPath = eventDataSelector.XPathTerms.Count > 0 ? $"{systemSelector} and {eventDataSelector}" : systemSelector;

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

    public static List<LogOnRecord> GetRecentDomainLogons(TimeSpan timeSpan, bool includeSystemLogons = false, bool includeServiceLogons = false)
    {
        return GetRecentLogons(timeSpan, includeSystemLogons, includeServiceLogons)
            .Where(x => x.TargetUserSid!.IsDomainSubauthority() && (x.TargetUserSid!.IsLocalMachineDomain() == false))
            .ToList();
    }
}
