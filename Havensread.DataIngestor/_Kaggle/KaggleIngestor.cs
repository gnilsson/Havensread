using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Havensread.DataIngestor;

public sealed class KaggleIngestor
{
    private readonly Kaggle.Settings _kaggleSettings;
    private readonly string _solutionDir;

    public KaggleIngestor(Kaggle.Settings kaggleSettings, string solutionDir)
    {
        _kaggleSettings = kaggleSettings;
        _solutionDir = solutionDir;
    }

    public async Task RunAsync()
    {
        var outputDir = Path.Combine(_solutionDir, "seeddata", "kaggle");

        if (!Path.Exists(outputDir))
        {
            await DownloadCsvToDirectoryAsync(outputDir);
        }

        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        var booksJsonFolder = Path.Combine(outputDir, "booksJson");

        if (!Directory.Exists(booksJsonFolder))
        {
            Directory.CreateDirectory(booksJsonFolder);
        }

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(10);

        foreach (var book in YieldKaggleBook(outputDir))
        {
            await semaphore.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var booksJson = JsonSerializer.Serialize(book, serializerOptions);
                    var outputPath = Path.Combine(booksJsonFolder, $"{book.BookID}.json");
                    await File.WriteAllTextAsync(outputPath, booksJson);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }


    private IEnumerable<Kaggle.Book> YieldKaggleBook(string outputDir)
    {
        var csvPath = Directory.GetFiles(outputDir).FirstOrDefault();
        ArgumentNullException.ThrowIfNull(csvPath, "No CSV files found in directory");

        using var reader = new StreamReader(csvPath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            BadDataFound = context =>
            {
                Console.WriteLine($"Bad data found at field {context.Field}: {context.RawRecord}");
            }
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<Kaggle.BookMap>();

        foreach (var kaggleBook in csv.GetRecords<Kaggle.Book>())
        {
            yield return kaggleBook;
        }
    }

    private async Task DownloadCsvToDirectoryAsync(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var client = new HttpClient();
        client.BaseAddress = new Uri("https://www.kaggle.com/api/v1/");
        var credentialsBytes = Encoding.ASCII.GetBytes($"{_kaggleSettings.Username}:{_kaggleSettings.ApiKey}");
        var credentials = Convert.ToBase64String(credentialsBytes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var ownerSlug = "jealousleopard";
        var datasetSlug = "goodreadsbooks";
        var url = $"datasets/download/{ownerSlug}/{datasetSlug}";

        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        using var zipStream = await response.Content.ReadAsStreamAsync();
        using var zipArchive = new ZipArchive(zipStream);

        foreach (var entry in zipArchive.Entries)
        {
            if (entry.Name.EndsWith(".csv"))
            {
                var outputPath = Path.Combine(_solutionDir, entry.Name);
                entry.ExtractToFile(outputPath, overwrite: true);
                Console.WriteLine($"Extracted: {outputPath}");
            }
        }
    }
}
