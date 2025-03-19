using DataGenerator;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

//builder.AddOpenAIChatClient();
builder.AddOllamaChatClient();

var services = builder.Build().Services;

var outlines = await new NarrativeOutlineGenerator(services).GenerateAsync();

var bookIdeas = await new BookIdeaGenerator(services, outlines).GenerateAsync();
