namespace RaceDay.Api.Models;

public class Result
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
    public string ResultStatus { get; set; } = "Completed";

    public Enrolment Enrolment { get; set; } = null!;
}
