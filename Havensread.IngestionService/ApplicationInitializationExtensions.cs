using Havensread.Connector;
using Havensread.IngestionService.Apis;
using Havensread.IngestionService.Workers;
using Havensread.IngestionService.Workers.Book;
using Havensread.ServiceDefaults;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using OpenAI;
using Polly;
using System.ClientModel;
using System.Net;
using System.Net.Http.Headers;

namespace Havensread.IngestionService;

public static class ApplicationInitializationExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        var workerTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IWorker).IsAssignableFrom(t));

        var otelBuilder = services.AddOpenTelemetry();

        foreach (var workerType in workerTypes)
        {
            services.AddSingleton(typeof(IWorker), workerType);
            otelBuilder.WithTracing(tracing => tracing.AddSource(workerType.Name));
        }

        services.AddSingleton<WorkerCoordinator>();
        services.AddSingleton<IWorkerCoordinator>(sp => sp.GetRequiredService<WorkerCoordinator>());
        services.AddHostedService<WorkerCoordinator>(sp => sp.GetRequiredService<WorkerCoordinator>());

        services.AddSingleton<BookRequestGenerator>();
        services.AddScoped<BookIngestionHandler>();

        return services;
    }

    public static IHostApplicationBuilder AddAIServices(this IHostApplicationBuilder builder)
    {
        var githubToken = builder.Configuration.GetRequiredSection(SectionName.GitHub.ModelsToken).Value;
        ArgumentNullException.ThrowIfNull(githubToken, nameof(githubToken));

        var credential = new ApiKeyCredential(githubToken);
        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = new Uri("https://models.inference.ai.azure.com")
        };

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        var chatClient = ghModelsClient.AsChatClient("gpt-4o-mini");
        var embeddingGenerator = ghModelsClient.AsEmbeddingGenerator("text-embedding-3-small");

        builder.Services
            .AddChatClient(chatClient)
            .UseFunctionInvocation()
            .UseLogging()
            .UseOpenTelemetry();

        builder.Services.AddEmbeddingGenerator(embeddingGenerator);

        return builder;
    }

    public static IHostApplicationBuilder AddHttpClients(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddDefaultWebAgentHttpClient("book-searcher", (sp, client) =>
            {
                var settings = builder.Configuration.GetRequiredSection(GoogleSettings.SectionName).Get<GoogleSettings>();
                ArgumentNullException.ThrowIfNull(settings, nameof(settings));

                client.BaseAddress = new Uri($"https://www.googleapis.com/customsearch/v1?key={settings.Token}&cx={settings.SearchEngineId}");
            })
            .AddDefaultWebAgentHttpClient("jina-reader", (sp, client) =>
            {
                var settings = builder.Configuration.GetRequiredSection(JinaSettings.SectionName).Get<JinaSettings>();
                ArgumentNullException.ThrowIfNull(settings, nameof(settings));

                client.BaseAddress = new Uri("https://r.jina.ai/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
            });

        return builder;
    }

    private static IServiceCollection AddDefaultWebAgentHttpClient(
        this IServiceCollection services,
        string name,
        Action<IServiceProvider, HttpClient>? options = null)
    {
        services.AddHttpClient(name, (sp, client) =>
        {
            options?.Invoke(sp, client);
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("BookInfoBot/1.0 (+https://yourdomain.com/bot; yourname@email.com)");
            //client.DefaultRequestHeaders.Add("Referer", "https://yourdomain.com/");
            //client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.BackoffType = DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            options.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(10) };
            options.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(30) };
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(20);
        });

        return services;
    }
}
