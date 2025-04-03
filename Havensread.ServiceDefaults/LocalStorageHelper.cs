using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Havensread.ServiceDefaults;

public static class LocalStorageHelper
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public static async Task WriteToJsonDiskAsync<T>(
        IEnumerable<T> data,
        Func<T, string> idSelector,
        string outputDirName,
        CancellationToken cancellationToken)
    {
        var dataArray = data.ToArray();

        if (dataArray.Length == 0) return;

        var outputDir = Path.Combine(PathUtils.SolutionDirectory, DirectoryName.SeedData, outputDirName);

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        await Parallel.ForEachAsync(data, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount / 3,
            CancellationToken = cancellationToken
        }, async (entity, ct) =>
        {
            var path = Path.Combine(outputDir, $"{idSelector(entity)}.json");
            var json = JsonSerializer.Serialize(entity, s_jsonSerializerOptions);
            await File.WriteAllTextAsync(path, json, ct);
        });
    }

    public static async IAsyncEnumerable<T> ReadFromJsonDiskAsync<T>(
        string outputDirName,
        JsonSerializerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var outputDir = Path.Combine(PathUtils.SolutionDirectory, DirectoryName.SeedData, outputDirName);
        if (!Directory.Exists(outputDirName)) yield break;

        var files = Directory.EnumerateFiles(outputDirName, "*.json").ToArray();
        var batchSize = Environment.ProcessorCount / 3;

        for (int i = 0; i < files.Length; i += batchSize)
        {
            var batch = files.Skip(i).Take(batchSize);

            var tasks = batch.Select(async file =>
            {
                await using var stream = File.OpenRead(file);
                return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                if (result is not null)
                {
                    yield return result;
                }
            }
        }
    }
}
