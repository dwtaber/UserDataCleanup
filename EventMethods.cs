namespace UserDataCleanup;

public class EventMethods
{
    public static List<LogOnRecord> GetRecentLogons(TimeSpan timeSpan)
    {
        var milliseconds = timeSpan.TotalMilliseconds;
        var xPath = $"*[System[EventID=4624 and TimeCreated[timediff(@SystemTime) <= {milliseconds}]]]";
        var query = new EventLogQuery("Security", PathType.LogName, xPath) { ReverseDirection = true };
        var allResults = new List<LogOnRecord>();
        using (var reader = new EventLogReader(query))
        {
            for (var record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
            {
                allResults.Add(new LogOnRecord(record as EventLogRecord));
            }
        }
        return allResults.GroupBy(x => x.TargetUserSid)
                         .Select(group => group.First())
                         .ToList();
    }

    public static List<LogOnRecord> GetRecentDomainLogons(TimeSpan timeSpan)
    {
        return GetRecentLogons(timeSpan)
            .Where(x => x.TargetUserSid.AccountDomainSid != null)
            .ToList();
    }
}
