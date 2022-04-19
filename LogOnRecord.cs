namespace UserDataCleanup;

public class LogOnRecord
{
    public string? TargetUserName { get; set; }
    public SecurityIdentifier? TargetUserSid { get; set; }
    public DateTime? TimeCreated { get; set; }
    

    public LogOnRecord(EventLogRecord record)
    {
        TargetUserName = record.GetPropertyValue("TargetUserName") as string;
        TargetUserSid = record.GetPropertyValue("TargetUserSid") as SecurityIdentifier;
        TimeCreated = record.TimeCreated;
    }
}
