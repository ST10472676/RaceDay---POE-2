namespace RaceDay.Api.Models;

public class Event
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public int RouteID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }

    public User Organiser { get; set; } = null!;
    public Route Route { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<EventWeather> WeatherRecords { get; set; } = new List<EventWeather>();
}
