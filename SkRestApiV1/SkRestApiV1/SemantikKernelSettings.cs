using System.ComponentModel.DataAnnotations;


public class SemanticKernelSettings
{
    public List<Model> Models { get; init; } = new();
    public KernelSetup KernelSetup { get; init; } = KernelSetup.MoreModelsInSameKernelRegistration;
}

public enum KernelSetup
{
    MoreModelsInSameKernelRegistration,
    KeyedKernels
}

public class Model
{
  
    public ModelCategory? Category { get; init; }
  
    public string DeploymentName { get; init; } = "";
   
    public string Url { get; init; } = "";
  
    public string ApiKeyName { get; init; } = "";
    public string ModelId { get; init; } = "";
}

public enum ModelCategory
{
    None,
    AzureOpenAi
}
