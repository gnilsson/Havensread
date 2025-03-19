using Havensread.Api.Endpoints;

namespace Havensread.Api.ServiceConfiguration;

public static class PipelineExecutionExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var books = app.MapGroup("books");
        books.MapGet("", async (/*GetBooks.Request request, */HttpContext context, GetBooks.Endpoint endpoint, CancellationToken cancellationToken) =>
        {
            return await endpoint.HandleAsync(null, context, cancellationToken);
        }).WithName(RoutingNames.Endpoint.GetBooks);

        return app;
    }

    public static WebApplication UseMiddlewares(this WebApplication app)
    {
        return app;
    }
}