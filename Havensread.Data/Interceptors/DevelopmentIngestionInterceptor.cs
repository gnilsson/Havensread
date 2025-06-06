﻿using Havensread.Data.Ingestion;
using Havensread.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Havensread.Data.Interceptors;

public sealed class DevelopmentIngestionInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var entities = eventData.Context!.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified && x.Entity is IngestedDocument)
            .Select(x => (IngestedDocument)x.Entity);

        await LocalStorageHelper.WriteJsonAsync(entities, e => e.Id.ToString(), DirectoryName.IngestedDocuments, cancellationToken);

        return result;
    }
}
