using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace HelloDotNet5
{
    public class WeatherClient
    {
        private readonly HttpClient httpclient;
        private readonly ServiceSettings setting;

        public WeatherClient(HttpClient httpclient,IOptions<ServiceSettings> setting)
        {
            this.httpclient = httpclient;
            this.setting = setting.Value;
        }

        public record Weather (string description);
        public record Main(decimal temp);
        public record Forecast(Weather[] weather,Main main, long dt);

        public async Task<Forecast> GetCurrentWeatherAsync(string city){
            try {
                var forcast = await httpclient.GetFromJsonAsync<Forecast>($"http://{setting.ApiUrl}/data/2.5/weather?q={city}&appid={setting.ApiKey}&units=metric");
                return forcast;
            }
            catch(Exception ex){
                return null;
            }

        }
    }
}