using Havensread.Api.Endpoints;
using Havensread.ServiceDefaults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OpenAI;
using Polly;
using Qdrant.Client;
using System.ClientModel;
using System.Net;
using System.Net.Http.Headers;

namespace Havensread.Api.ServiceConfiguration;

public static class ApplicationInitializationExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.AddScoped<GetBooks.Endpoint>();
        return services;
    }

    //public static IServiceCollection AddIngestionPipeline(this IServiceCollection services)
    //{
    //    services.AddScoped<BookIngestionDataCollector>();
    //    //services.AddHostedService<IngestionBackgroundService>();
    //    return services;
    //}

    //public static IHostApplicationBuilder AddAIServices(this IHostApplicationBuilder builder)
    //{
    //    var githubToken = builder.Configuration.GetRequiredSection(SectionName.GitHub.ModelsToken).Value;
    //    ArgumentNullException.ThrowIfNull(githubToken, nameof(githubToken));

    //    var credential = new ApiKeyCredential(githubToken);
    //    var openAIOptions = new OpenAIClientOptions()
    //    {
    //        Endpoint = new Uri("https://models.inference.ai.azure.com")
    //    };

    //    var ghModelsClient = new OpenAIClient(credential, openAIOptions);
    //    var chatClient = ghModelsClient.AsChatClient("gpt-4o-mini");
    //    var embeddingGenerator = ghModelsClient.AsEmbeddingGenerator("text-embedding-3-small");

    //    builder.Services
    //        .AddChatClient(chatClient)
    //        .UseFunctionInvocation()
    //        .UseLogging()
    //        .UseOpenTelemetry();
    //    builder.Services.AddEmbeddingGenerator(embeddingGenerator);

    //    return builder;
    //}


    //public static IHostApplicationBuilder AddHttpClients(this IHostApplicationBuilder builder)
    //{
    //    builder.Services
    //        .AddDefaultWebAgentHttpClient("book-searcher", (sp, options) =>
    //        {
    //            var settings = builder.Configuration.GetRequiredSection(GoogleSettings.SectionName).Get<GoogleSettings>();
    //            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

    //            options.BaseAddress = new Uri($"https://www.googleapis.com/customsearch/v1?key={settings.Token}&cx={settings.SearchEngineId}");
    //        })
    //        .AddDefaultWebAgentHttpClient("jina-reader", (sp, options) =>
    //        {
    //            var settings = builder.Configuration.GetRequiredSection(JinaSettings.SectionName).Get<JinaSettings>();
    //            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

    //            options.BaseAddress = new Uri("https://r.jina.ai/");
    //            options.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
    //        });

    //    return builder;
    //}

    //private static IServiceCollection AddDefaultWebAgentHttpClient(
    //    this IServiceCollection services,
    //    string name,
    //    Action<IServiceProvider, HttpClient>? options = null)
    //{
    //    services.AddHttpClient(name, (sp, client) =>
    //    {
    //        options?.Invoke(sp, client);
    //        //client.DefaultRequestHeaders.UserAgent.ParseAdd("BookInfoBot/1.0 (+https://yourdomain.com/bot; yourname@email.com)");
    //        //client.DefaultRequestHeaders.Add("Referer", "https://yourdomain.com/");
    //        //client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
    //        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    //        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
    //        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
    //        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
    //        client.Timeout = TimeSpan.FromMinutes(2);
    //    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    //    {
    //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
    //    })
    //    .AddStandardResilienceHandler(options =>
    //    {
    //        options.Retry.BackoffType = DelayBackoffType.Exponential;
    //        options.Retry.UseJitter = true;
    //    });

    //    return services;
    //}
}
