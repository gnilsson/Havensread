using Havensread.Api.Endpoints;
using Havensread.Api.ErrorHandling;

namespace Havensread.Api.ServiceConfiguration;

public static class PipelineInitializationExtensions
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
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async context =>
            {
                await using var scope = errApp.ApplicationServices.CreateAsyncScope();
                var exceptionHandler = scope.ServiceProvider.GetRequiredService<ExceptionHandler>();

                await exceptionHandler.HandleExceptionAsync(context);
            });
        });

        return app;
    }
}