using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        var userId = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        return userId switch
        {
            "1" => data[..3],
            "2" => data[..2],
            _ => new List<WeatherForecast>()
        };
    }
    
    
    [Authorize(Roles = "PremiumUser")]
    [HttpPost(Name = "CreateWeatherForecast")]
    public ActionResult<WeatherForecast> Create([FromBody] WeatherForecast forecast)
    {
        var userId = User.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        
        var newForecast = new WeatherForecast
        {
            Date = forecast.Date,
            TemperatureC = forecast.TemperatureC,
            Summary = $"{forecast.Summary ?? String.Empty} Manually created by user {userId}"
        };
        return CreatedAtRoute("CreateWeatherForecast", new { id = Guid.NewGuid() }, newForecast);
    }
}