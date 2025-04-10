using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SkRestApiV1;

public class SemanticKernelSettingsValidation : IValidateOptions<SemanticKernelsSettings>
{
  
    public SemanticKernelSettingsValidation()
    {
    }

    public ValidateOptionsResult Validate(string name, SemanticKernelsSettings semanticKernelSettings)
    {
        if (semanticKernelSettings == null)
        {
            var failureReason = "SemanticKernelSettings is null";
            return ValidateOptionsResult.Fail(failureReason);
        }
        if (semanticKernelSettings.Kernels.Count == 0)
        {
            var failureReason = "c.KernelSettings.Count== 0";
            return ValidateOptionsResult.Fail(failureReason);
        }
        var defaultKernels = semanticKernelSettings.Kernels.Where(k => k.IsDefault).ToList();
        if (defaultKernels.Count==0)
        {
            var failureReason = "c.Kernels.IsDefault.Count==0";
            return ValidateOptionsResult.Fail(failureReason);
        }
        if (defaultKernels.Count >1)
        {
            var failureReason = $"c.Kernels.IsDefault.Count> 1 {defaultKernels.Count }";
            return ValidateOptionsResult.Fail(failureReason);
        }
        foreach (var kernelSettings in semanticKernelSettings.Kernels.Select((kernel, kernelIndex) => new { kernel, kernelIndex = kernelIndex }))
        {
            var defaultModel = kernelSettings.kernel.Models.Where(k => k.IsDefault).ToList();
            if (defaultModel.Count == 0)
            {
                var failureReason = $"kernelSettings name {kernelSettings.kernel.Name} has no default model ";
                return ValidateOptionsResult.Fail(failureReason);
            }
            if (defaultModel.Count > 1)
            {
                var failureReason = $"kernelSettings name {kernelSettings.kernel.Name} has {defaultModel.Count} default models";
                return ValidateOptionsResult.Fail(failureReason);
            }
            foreach (var model in kernelSettings.kernel.Models.Select((modelValue, modelIndex) => new {value = modelValue, index = modelIndex }))
            {
                if (model.value == null)
                {
                    var failureReason = $"model==null for kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
                if (model.value.Category == ModelCategory.None)
                {
                    var failureReason = $"model.Category == ModelCategory.None kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
                if (string.IsNullOrWhiteSpace(model.value.DeploymentName))
                {
                    var failureReason = $"string.IsNullOrWhiteSpace(model.DeploymentName) kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
                if (string.IsNullOrWhiteSpace(model.value.Url))
                {
                    var failureReason = $"string.IsNullOrWhiteSpace(model.Url) for kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
                if (string.IsNullOrWhiteSpace(model.value.ApiKeyName))
                {
                    var failureReason = $"string.IsNullOrWhiteSpace(model.ApiKeyName) for kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
                if (kernelSettings.kernel.Models.Count >1 && string.IsNullOrWhiteSpace(model.value.ServiceId))
                {
                    var failureReason = $"kernelSettings.kernel.Models.Count >1 && string.IsNullOrWhiteSpace(model.ServiceId) for kernelIndex {kernelSettings.kernelIndex} , modelIndex {model.index}";
                    return ValidateOptionsResult.Fail(failureReason);
                }
            }
        }
        return ValidateOptionsResult.Success;
    }
}