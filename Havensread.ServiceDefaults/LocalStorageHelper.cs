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

        var outputDir = Path.Combine(
            PathUtils.SolutionDirectory,
            "seeddata",
            outputDirName);

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        await Parallel.ForEachAsync(data, new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2,
            CancellationToken = cancellationToken
        }, async (entity, ct) =>
        {
            var path = Path.Combine(outputDir, $"{idSelector(entity)}.json");
            var json = JsonSerializer.Serialize(entity, s_jsonSerializerOptions);
            await File.WriteAllTextAsync(path, json, ct);
        });
    }

    public static async IAsyncEnumerable<T> ReadFromJsonDiskAsync<T>(
        string jsonDir,
        JsonSerializerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!Directory.Exists(jsonDir)) yield break;

        var files = Directory.EnumerateFiles(jsonDir, "*.json").ToArray();
        var batchSize = Environment.ProcessorCount / 2;

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
