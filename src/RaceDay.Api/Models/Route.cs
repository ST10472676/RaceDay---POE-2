namespace RaceDay.Api.Models;

public class Route
{
    public int RouteID { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public string RouteDirection { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string? Description { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
