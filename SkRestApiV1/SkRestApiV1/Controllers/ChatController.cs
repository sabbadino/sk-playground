using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.Text;

namespace SkRestApiV1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
      

        private readonly ILogger<ChatController> _logger;
        private readonly IOptions<SemanticKernelSettings> _semanticKernelSettings;
        private readonly Kernel _kernel;

        public ChatController(ILogger<ChatController> logger, IOptions<SemanticKernelSettings> semanticKernelSettings, Kernel kernel)
        {
            _logger = logger;
            _semanticKernelSettings = semanticKernelSettings;
            _kernel = kernel;
        }

        [HttpPost(template:"chat", Name = "Chat" )]
        public async Task<ActionResult<string>> Chat([FromBody] UserQuestion question)
        {
            var history = $"""
                           <message role="system">Use lots of emojis when u answer any question</message>
                           <message role="user">{question.UserPrompt}</message>
                           """;
            var promptExecutionSettings = new PromptExecutionSettings { ModelId = "pippo" };
            if (!string.IsNullOrWhiteSpace(question.ModelId)) {
                if (_semanticKernelSettings.Value.Models.Any(m => m.ModelId == question.ModelId))
                {
                    promptExecutionSettings.ModelId = question.ModelId;
                }
                else
                {
                    return BadRequest($"ModelId {question.ModelId} not found"); 
                }
            }
            var response = await _kernel.InvokePromptAsync(history, new KernelArguments(promptExecutionSettings));

            return response.ToString();
        }
    }

    public class UserQuestion   
    {
        public string UserPrompt { get; init; } = "";
        public string ModelId { get; init; } = "";
    }
}
