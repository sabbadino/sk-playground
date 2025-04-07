using Microsoft.SemanticKernel;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Options;
using SkRestApiV1;
using Microsoft.Extensions.Configuration;
using SkRestApiV1.Controllers;
using System.Collections.Immutable;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Version = "1.0.0",
}));

builder.Services.AddOptions<SemanticKernelsSettings>()
    .BindConfiguration(nameof(SemanticKernelsSettings))
    //.ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions
    <SemanticKernelsSettings>, SemanticKernelSettingsValidation>();

var semanticKernelSettingsSection = builder.Configuration.GetSection(nameof(SemanticKernelsSettings));
var semanticKernelSettings = semanticKernelSettingsSection.Get<SemanticKernelsSettings>();
if (semanticKernelSettings == null)
{
    throw new Exception("semanticKernelSettings == null");
}
if(semanticKernelSettings.Kernels.Count == 0)
{
    throw new Exception("No models found in Kernels configuration");
}


foreach (var kernelSetting in semanticKernelSettings.Kernels)
{
    var skBuilder = Kernel.CreateBuilder();
    foreach (var model in kernelSetting.Models.Where(m => m.Category == ModelCategory.AzureOpenAi))
    {
        var apiKeyName = builder.Configuration[model.ApiKeyName];
        if (string.IsNullOrEmpty(apiKeyName))
        {
            throw new Exception($"Could not find value for key {apiKeyName}");
        }
        skBuilder.AddAzureOpenAIChatCompletion(model.DeploymentName, model.Url, apiKeyName, serviceId: model.ServiceId);
        skBuilder.Services.AddLogging(l => l.SetMinimumLevel(LogLevel.Debug).AddConsole());
    }
    builder.Services.AddTransient((_) => new KernelWrapper { Kernel = skBuilder.Build(), Name = kernelSetting.Name, ServiceIds = kernelSetting.Models.Select(m=> m.ServiceId).ToImmutableList()});

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