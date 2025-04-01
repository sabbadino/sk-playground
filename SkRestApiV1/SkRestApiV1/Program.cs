using Microsoft.SemanticKernel;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Options;
using SkRestApiV1;

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

//builder.Services.AddKernel().AddAzureOpenAIChatCompletion(;
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