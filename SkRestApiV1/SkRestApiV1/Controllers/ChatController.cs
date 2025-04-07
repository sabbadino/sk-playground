using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using OpenAI.Chat;
#pragma warning disable SKEXP0001

namespace SkRestApiV1.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
      

        private readonly ILogger<ChatController> _logger;
        private readonly SemanticKernelsSettings _semanticKernelSettings;
        private readonly IEnumerable<KernelWrapper> _kernelWrappers;

        public ChatController(ILogger<ChatController> logger, IOptions<SemanticKernelsSettings> semanticKernelSettings,
            IEnumerable<KernelWrapper> kernelWrappers)
        {
            _logger = logger;
            _semanticKernelSettings = semanticKernelSettings.Value;
            _kernelWrappers = kernelWrappers;
        }

        [HttpPost(template:"ask", Name = "Ask")]
        public async Task<ActionResult<string>> Ask([FromBody] UserQuestion question)
        {
            if(string.IsNullOrEmpty(question.KernelName) && !string.IsNullOrEmpty(question.ServiceId))
            {
                return BadRequest($"ServiceId {question.ServiceId} is not valid without a KernelName"); 
            }
            var history = new ChatHistory();
            history.AddSystemMessage("Use lots of emojis when u answer any question");
            history.AddUserMessage(question.UserPrompt);

            var defaultKernelName = _semanticKernelSettings.Kernels.Single(k => k.IsDefault).Name;
            var kernelWrapper = _kernelWrappers.SingleOrDefault(k => k.Name == defaultKernelName);
            ArgumentNullException.ThrowIfNull(kernelWrapper, $"Default kernel {defaultKernelName} not found");
            var promptExecutionSettings = new PromptExecutionSettings ();
            string? serviceId = null;
            if (!string.IsNullOrWhiteSpace(question.KernelName)) {
                kernelWrapper = _kernelWrappers.SingleOrDefault(k => k.Name == question.KernelName);
                if (kernelWrapper!=null)
                {
                    if (!string.IsNullOrWhiteSpace(question.ServiceId))
                    {
                        var modelId = kernelWrapper.ServiceIds.SingleOrDefault(m => m == question.ServiceId);
                        if (modelId!=null)
                        {
                            serviceId = question.ServiceId;
                        }
                        else
                        {
                            return BadRequest($"ModelId {question.ServiceId} not found");
                        }
                    }
                }
                else
                {
                    return BadRequest($"KernelName {question.KernelName} not found"); 
                }
            }
            var c = kernelWrapper.Kernel.GetRequiredService<IChatCompletionService>(serviceId);
            
            var response = await c.GetChatMessageContentAsync(history, promptExecutionSettings, kernelWrapper.Kernel);  
            //await get .InvokePromptAsync(history, new KernelArguments(promptExecutionSettings));

            return response.ToString();
        }
    }

    public class UserQuestion   
    {
        public string UserPrompt { get; init; } = "";
        public string KernelName { get; init; } = "";
        public string ServiceId{ get; init; } = "";
    }
}
