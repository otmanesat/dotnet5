using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HelloDotNet5.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherClient client;
        public WeatherForecastController(ILogger<WeatherForecastController> logger,IWeatherClient client)
        {
            _logger = logger;
            this.client = client;
        }

        [HttpGet("{city}")]
        public async  Task<IActionResult> Get(string city)
        {
            if(string.IsNullOrEmpty(city)){
                return BadRequest();
            }
            var result = await client.GetCurrentWeatherAsync(city);
            if(result is null){
                return NotFound();
            }
            return Ok(new WeatherForecast{
                Summary = result.weather[0].description,
                TemperatureC = (int)result.main.temp,
                Date = DateTimeOffset.FromUnixTimeSeconds(result.dt).DateTime
            });
        }
    }
}
