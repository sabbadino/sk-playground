using System.ComponentModel.DataAnnotations;


public class SemanticKernelSettings
{
    public List<Model> Models { get; init; } = new();
}

public class Model
{
  
    public ModelCategory? Category { get; init; }
  
    public string DeploymentName { get; init; } = "";
   
    public string Url { get; init; } = "";
  
    public string ApiKeyName { get; init; } = "";
    public string? modelId { get; init; } = "";
}

public enum ModelCategory
{
    None,
    AzureOpenAi
}
