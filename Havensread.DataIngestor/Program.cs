using Havensread.DataIngestor;
using Havensread.ServiceDefaults;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var kaggleApiKey = builder.GetRequiredSection(Kaggle.Settings.SectionName).Get<Kaggle.Settings>()!;

var solutionDir = PathUtils.FindAncestorDirectoryContaining("*.sln");

var kaggleIngestor = new KaggleIngestor(kaggleApiKey, solutionDir);
await kaggleIngestor.RunAsync();
