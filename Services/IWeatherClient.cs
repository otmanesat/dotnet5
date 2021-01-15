using System.Threading.Tasks;

namespace HelloDotNet5
{
    public interface IWeatherClient {
        Task<WeatherClient.Forecast> GetCurrentWeatherAsync (string city);
    }
}