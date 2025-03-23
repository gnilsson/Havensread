using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Havensread.Api.Ingestion;

public sealed class DevelopmentInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var entities = eventData.Context!.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified && x.Entity is IngestedDocument)
            .Select(x => (IngestedDocument)x.Entity);

        var slnDir = PathUtils.FindAncestorDirectoryContaining("*.sln");

        foreach (var entity in entities)
        {
            var path = Path.Combine(slnDir, "seeddata", "ingestedDocuments", $"{entity.Id}.json");
            var json = JsonSerializer.Serialize(entity, s_jsonSerializerOptions);
            await File.WriteAllTextAsync(path, json, cancellationToken);
        }

        return result;
    }
}
