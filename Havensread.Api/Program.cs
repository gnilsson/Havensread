using Havensread.Api.ErrorHandling;
using Havensread.Api.ServiceConfiguration;
using Havensread.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.AddServiceDefaults();

builder.AddDatabase();

builder.AddVectorStore();

//builder.AddAIServices();

//builder.AddHttpClients();

builder.Services.AddScoped<ExceptionHandler>();

builder.Services.AddEndpoints();

//builder.Services.AddIngestionPipeline();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddlewares();

app.MapEndpoints();

app.UseMiddlewares();

app.UseHttpsRedirection();

//app.MapHub

app.Run();
