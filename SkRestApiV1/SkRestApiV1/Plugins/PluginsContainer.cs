using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace SkRestApiV1.Plugins
{
    public class PluginsContainer 
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PluginsContainer(IHttpClientFactory httpClientFactory,IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }   
        [KernelFunction("get_current_weather")]
        [Description("Returns the current weather for the specified location")]
        public async Task<GetWeatherResponse> GetCurrentWeather(GetCurrentWeatherRequest getCurrentWeatherRequest)
        {
            var client = _httpClientFactory.CreateClient();
            var ret = await client.GetAsync($"http://api.weatherstack.com/current?access_key={_configuration["weatherkey"]}&query={getCurrentWeatherRequest.Location}&units=m");
            if (!ret.IsSuccessStatusCode)
            {
                throw new Exception($"{ret.StatusCode} + {await ret.Content.ReadAsStringAsync()}");
            }
            return JsonSerializer.Deserialize<GetWeatherResponse>(await ret.Content.ReadAsStreamAsync()) ?? new();
        }
    }
    public class GetCurrentWeatherRequest
    {
        [Description("The location (town or region) name. IMPORTANT : Assistant must ask the user a value for location. If not provided in the conversation, Assistant must not not make up one")]

        public string Location { get; init; } = "";
    }
    public class Current
    {
        public string observation_time { get; set; }
        public float temperature { get; set; }
        public int weather_code { get; set; }
        public List<string> weather_icons { get; set; }
        public List<string> weather_descriptions { get; set; }
        public float wind_speed { get; set; }
        public float wind_degree { get; set; }
        public string wind_dir { get; set; }
        public float pressure { get; set; }
        public float precip { get; set; }
        public float humidity { get; set; }
        public float cloudcover { get; set; }
        public float feelslike { get; set; }
        public float uv_index { get; set; }
        public float visibility { get; set; }
    }

    public class Location
    {
        public string name { get; set; }
        public string country { get; set; }
        public string region { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string timezone_id { get; set; }
        public string localtime { get; set; }
        public int localtime_epoch { get; set; }
        public string utc_offset { get; set; }
    }

    public class Request
    {
        public string type { get; set; }
        public string query { get; set; }
        public string language { get; set; }
        public string unit { get; set; }
    }

    public class GetWeatherResponse
    {
        public Request request { get; set; }
        public Location location { get; set; }
        public Current current { get; set; }
    }
}
