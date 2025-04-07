using System.ComponentModel.DataAnnotations;


public class SemanticKernelsSettings
{
    
    public List<KernelSettings> Kernels { get; init; } = new();
}

public class KernelSettings
{
    public bool IsDefault { get; init; } = false;   
    public string Name { get; init; }
    public List<Model> Models { get; init; } = new();
}




public class Model
{
    public bool IsDefault { get; init; } = false;
    public ModelCategory? Category { get; init; }
  
    public string DeploymentName { get; init; } = "";
   
    public string Url { get; init; } = "";
  
    public string ApiKeyName { get; init; } = "";
    public string ServiceId { get; init; } = "";
}

public enum ModelCategory
{
    None,
    AzureOpenAi
}
