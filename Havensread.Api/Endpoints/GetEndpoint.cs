using Havensread.Data.App;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Havensread.Api.Endpoints;

public sealed class GetBooks
{
    public sealed class Request
    {

    }

    public sealed class Response
    {

    }

    public sealed class Endpoint : GetEndpoint
    {
        private readonly AppDbContext _dbContext;

        public Endpoint(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IResult> HandleAsync(Request request, HttpContext _, CancellationToken cancellationToken)
        {
            var query = _dbContext.Books
                .AsNoTracking()
                .Where(x => x.PublicationDate != DateTime.MinValue)
                .Where(x => x.Title.Contains("Harry Potter"));

            var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

            return Task.FromResult(Results.Stream(async (stream) =>
            {
                await foreach (var book in query.Take(50).AsAsyncEnumerable())
                {
                    await JsonSerializer.SerializeAsync(stream, book, serializerOptions, cancellationToken);
                    await stream.FlushAsync();
                }
            }));
        }
    }
}

public abstract class GetEndpoint
{
}
