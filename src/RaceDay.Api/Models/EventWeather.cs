namespace RaceDay.Api.Models;

public class EventWeather
{
    public int WeatherID { get; set; }
    public int EventID { get; set; }
    public DateTime WeatherDate { get; set; }
    public decimal? Temperature { get; set; }
    public string? WeatherCondition { get; set; }
    public int? Humidity { get; set; }

    public Event Event { get; set; } = null!;
}
