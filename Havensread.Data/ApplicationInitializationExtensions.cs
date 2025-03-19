using Havensread.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havensread.Data;

public static class ApplicationInitializationExtensions
{
    public static IHostApplicationBuilder AddDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>("havensread-pgsql", null, options =>
        {
            options.UseNpgsql(o =>
            {
                o.EnableRetryOnFailure();
                o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "app");
            })
            .UseSnakeCaseNamingConvention();

            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return builder;
    }
}
