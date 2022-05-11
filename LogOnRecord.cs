namespace UserDataCleanup;

public class LogOnRecord
{
    public string? TargetUserName { get; set; }
    public SecurityIdentifier? TargetUserSid { get; set; }
    public LogonType LogonType { get; set; }
    public DateTime? TimeCreated { get; set; }
    

    public LogOnRecord(EventLogRecord record)
    {
        TargetUserName = record.GetPropertyValue("TargetUserName") as string;
        TargetUserSid = record.GetPropertyValue("TargetUserSid") as SecurityIdentifier;
        LogonType = (LogonType)Convert.ToInt32(record.GetPropertyValue("LogonType"));
        TimeCreated = record.TimeCreated;
    }
}
