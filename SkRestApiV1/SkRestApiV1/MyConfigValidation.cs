using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SkRestApiV1;

public class SemanticKernelSettingsValidation : IValidateOptions<SemanticKernelSettings>
{
  
    public SemanticKernelSettingsValidation()
    {
    }

    public ValidateOptionsResult Validate(string name, SemanticKernelSettings semanticKernelSettings)
    {
        if (semanticKernelSettings == null)
        {
            var failureReason = "SemanticKernelSettings is null";
            return ValidateOptionsResult.Fail(failureReason);
        }
        if (semanticKernelSettings.Models.Count == 0)
        {
            var failureReason = "c.Models.Count== 0";
            return ValidateOptionsResult.Fail(failureReason);
        }
        foreach (var model in semanticKernelSettings.Models.Select((value, index) => new { value, index }))
        {
            if(model.value ==null)
            {
                var failureReason = $"model==null for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (model.value.Category == ModelCategory.None)
            {
                var failureReason = $"model.Category == ModelCategory.None for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (string.IsNullOrWhiteSpace(model.value.DeploymentName))
            {
                var failureReason = $"string.IsNullOrWhiteSpace(model.DeploymentName) for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (string.IsNullOrWhiteSpace(model.value.Url))
            {
                var failureReason = $"string.IsNullOrWhiteSpace(model.Url) for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (string.IsNullOrWhiteSpace(model.value.ApiKeyName))
            {
                var failureReason = $"string.IsNullOrWhiteSpace(model.ApiKeyName) for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (string.IsNullOrWhiteSpace(model.value.ModelId))
            {
                var failureReason = $"string.IsNullOrWhiteSpace(model.modelId) for index {model.index}";
                return ValidateOptionsResult.Fail(failureReason);
            }
        }
        return ValidateOptionsResult.Success;
    }
}