using Microsoft.SemanticKernel;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Options;
using SkRestApiV1;
using Microsoft.Extensions.Configuration;
using SkRestApiV1.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string failureReason = "";
builder.Services.AddOptions<SemanticKernelSettings>()
    .BindConfiguration(nameof(SemanticKernelSettings))
    //.ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions
    <SemanticKernelSettings>, SemanticKernelSettingsValidation>();

var semanticKernelSettingsSection = builder.Configuration.GetSection(nameof(SemanticKernelSettings));
var semanticKernelSettings = semanticKernelSettingsSection.Get<SemanticKernelSettings>();
if (semanticKernelSettings == null)
{
    throw new Exception("semanticKernelSettings == null");
}
if(semanticKernelSettings.Models.Count == 0)
{
    throw new Exception("No models found in SemanticKernelSettings configuration");
}

if (semanticKernelSettings.KernelSetup == KernelSetup.MoreModelsInSameKernelRegistration)
{
    var skBuilder = builder.Services.AddKernel();
    foreach (var model in semanticKernelSettings.Models.Where(m => m.Category == ModelCategory.AzureOpenAi))
    {
        var apiKeyName = builder.Configuration[model.ApiKeyName];
        if (string.IsNullOrEmpty(apiKeyName))
        {
            throw new Exception($"Could not find value for key {apiKeyName}");
        }
        skBuilder.AddAzureOpenAIChatCompletion(model.DeploymentName, model.Url, apiKeyName, modelId: model.ModelId);
    }
    skBuilder.Services.AddLogging(l => l.SetMinimumLevel(LogLevel.Debug).AddConsole());
}
if (semanticKernelSettings.KernelSetup == KernelSetup.KeyedKernels)
{
    foreach (var model in semanticKernelSettings.Models.Where(m => m.Category == ModelCategory.AzureOpenAi))
    {
        var skBuilder = builder.Services.AddAddKeyedKernel(model.ModelId);
        var apiKeyName = builder.Configuration[model.ApiKeyName];
        if (string.IsNullOrEmpty(apiKeyName))
        {
            throw new Exception($"Could not find value for key {apiKeyName}");
        }
        skBuilder.AddAzureOpenAIChatCompletion(model.DeploymentName, model.Url, apiKeyName, modelId: model.ModelId);
        skBuilder.Services.AddLogging(l => l.SetMinimumLevel(LogLevel.Debug).AddConsole());

    }
}
else
{
    throw new Exception($"KernelSetup not supported: {semanticKernelSettings.KernelSetup}");
}   
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

