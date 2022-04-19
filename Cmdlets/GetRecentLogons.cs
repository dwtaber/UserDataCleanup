namespace UserDataCleanup;

[Cmdlet(verbName:VerbsCommon.Get, nounName:"RecentLogons", DefaultParameterSetName = "FromDays")]
public class GetRecentLogons : PSCmdlet
{
    #region Properties/Fields

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromDays")]
    [ValidateNotNullOrEmpty]
    public double MaxDays { get; set; }

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromDate")]
    [ValidateNotNullOrEmpty]
    public DateTime FromDate { get; set; }

    [Parameter()]
    public SwitchParameter IncludeNonDomainAccounts { get; set; }

    internal TimeSpan TimeSpan { get; set; }

    #endregion

    protected override void BeginProcessing()
    {
        // if (ParameterSetName == "FromDays")
        // {
        //     TimeSpan = TimeSpan.FromDays(MaxDays);
        // }
        // 
        // if (ParameterSetName == "FromDate")
        // {
        //     TimeSpan = DateTime.Today - FromDate;
        // }
        TimeSpan = ParameterSetName switch
        {
            "FromDays" => TimeSpan.FromDays(MaxDays),
            "FromDate" => DateTime.Today - FromDate,
            _ => throw new NotImplementedException()
        };
    }

    protected override void ProcessRecord()
    {
        var result = IncludeNonDomainAccounts ? EventMethods.GetRecentLogons(TimeSpan) : EventMethods.GetRecentDomainLogons(TimeSpan);
        WriteObject(result, enumerateCollection: true);
    }

    protected override void EndProcessing()
    {

    }
}
