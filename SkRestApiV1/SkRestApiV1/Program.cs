using Microsoft.SemanticKernel;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Options;
using SkRestApiV1;
using Microsoft.Extensions.Configuration;

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
var skBuilder = builder.Services.AddKernel();
foreach(var model in semanticKernelSettings.Models.Where(m=> m.Category ==ModelCategory.AzureOpenAi))
{
    var apiKeyName = builder.Configuration[model.ApiKeyName];
    if (string.IsNullOrEmpty(apiKeyName))
    {
        throw new Exception($"Couldnot find value for key {apiKeyName}");
    }
    skBuilder.AddAzureOpenAIChatCompletion(model.DeploymentName, model.Url, apiKeyName,modelId: model.modelId);
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

string GetIt() {
    return failureReason;   
}