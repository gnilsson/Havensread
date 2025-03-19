using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;

namespace DataGenerator;

internal static class ServiceConfigurationExtensions
{
    public static ChatClientBuilder AddOllamaChatClient(this IHostApplicationBuilder builder)
    {
        var chatClientBuilder = builder.Services.AddChatClient(sp =>
        {
            var httpClient = sp.GetService<HttpClient>() ?? new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            return new OllamaChatClient(new Uri("http://localhost:11434"), "gemma3:12b", httpClient);
        });

        return chatClientBuilder
            //     .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true);
    }

    public static ChatClientBuilder AddOpenAIChatClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<OpenAIConfiguration>()
            .Bind(builder.Configuration.GetSection(nameof(OpenAIConfiguration)))
            .ValidateOnStart();

        builder.Services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;

            return new OpenAIClient(new ApiKeyCredential(options.ApiKey));
        });

        return builder.Services
            .AddChatClient(builder => builder.GetRequiredService<OpenAIClient>().AsChatClient(OpenAIModels.Gpt4oMini))
            .UseFunctionInvocation()
            .UseOpenTelemetry(configure: c => c.EnableSensitiveData = true);
    }
}

public sealed class OpenAIConfiguration
{
    public required string ApiKey { get; init; }
}

public static class OpenAIModels
{
    public const string Gpt35Turbo = "gpt-3.5-turbo";
    public const string Gpt35TurboInstruct = "gpt-3.5-turbo-instruct";
    public const string Gpt4 = "gpt-4";
    public const string Gpt41106Previw = "gpt-4-1106-preview";
    public const string Gpt4o = "gpt-4o";
    public const string Gpt4oMini = "gpt-4o-mini";
}


