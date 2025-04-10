using Microsoft.SemanticKernel;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Options;
using SkRestApiV1;
using Microsoft.Extensions.Configuration;
using SkRestApiV1.Controllers;
using System.Collections.Immutable;
using Microsoft.OpenApi.Models;
using SkRestApiV1.Plugins;
#pragma warning disable SKEXP0070
#pragma warning disable SKEXP0001

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

builder.Services.RegisterByConvention<Program>();
builder.Services.AddHttpClient();
var allPlugins = semanticKernelSettings.Kernels.SelectMany(k => k.Plugins).Distinct();
foreach (var pluginName in allPlugins)
{
    // build plugin using the global IOC container  
    builder.Services.AddKeyedSingleton(pluginName, (serviceProvider, _) =>
    {
        var type = Type.GetType($"SkRestApiV1.Plugins.{pluginName}");
        ArgumentNullException.ThrowIfNull(type, $"Plugin {pluginName} not found");
        return KernelPluginFactory.CreateFromType(type, pluginName, serviceProvider);
    });
}
// register by convention inside in the default asp.net core ServiceCollection
builder.Services.RegisterByConvention<Program>();

foreach (var kernelSetting in semanticKernelSettings.Kernels)
{
    builder.Services.AddTransient(globalServiceProvider => {
        // I do not new the Kernel since i need to call AddAzureOpenAIChatCompletion and similar
        var skBuilder = Kernel.CreateBuilder();
        foreach (var model in kernelSetting.Models.Where(m => m.Category == ModelCategory.AzureOpenAi))
        {
            var apiKeyName = builder.Configuration[model.ApiKeyName];
            if (string.IsNullOrEmpty(apiKeyName))
            {
                throw new Exception($"Could not find value for key {apiKeyName}");
            }
            skBuilder.AddAzureOpenAIChatCompletion(model.DeploymentName, model.Url, apiKeyName, serviceId: model.ServiceId);
        }
        foreach (var model in kernelSetting.Models.Where(m => m.Category == ModelCategory.Ollama))
        {
            skBuilder.AddOllamaChatCompletion(model.DeploymentName, new Uri(model.Url), serviceId: model.ServiceId);
        }
        skBuilder.Services.AddLogging(l => l.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var kernel = skBuilder.Build();
        foreach( var pluginName in kernelSetting.Plugins)
        {
            var plugin= globalServiceProvider.GetRequiredKeyedService<KernelPlugin>(pluginName);
            ArgumentNullException.ThrowIfNull(plugin, $"Plugin {pluginName} could not be cast to KernelPlugin");
            kernel.Plugins.Add(plugin);
        }
        return new KernelWrapper { SystemMessageName = kernelSetting.SystemMessageName,  Kernel = kernel, Name = kernelSetting.Name, ServiceIds = kernelSetting.Models.Select(m => m.ServiceId).ToImmutableList() };
    });
       

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