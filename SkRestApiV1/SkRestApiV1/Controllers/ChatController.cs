using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SkRestApiV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
      

        private readonly ILogger<ChatController> _logger;
        private readonly IOptions<SemanticKernelSettings> _semanticKernelSettings;

        public ChatController(ILogger<ChatController> logger, IOptions<SemanticKernelSettings> semanticKernelSettings)
        {
            _logger = logger;
            _semanticKernelSettings = semanticKernelSettings;
        }

        [HttpGet(template:"chat", Name = "Chat" )]
        public IEnumerable<WeatherForecast> Chat()
        {
            return new List<WeatherForecast>
            {
                new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = 25,
                    Summary = "Hot"
                }
            };  
        }
    }
}
