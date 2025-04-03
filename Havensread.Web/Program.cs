using Havensread.Connector;
using Havensread.Web.Components;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR(o =>
{
    if (builder.Environment.IsDevelopment())
    {
        o.EnableDetailedErrors = true;
    }
});
//builder.AddRabbitMQClient(connectionName: "havensread-rabbitmq");

//builder.Services.AddMassTransit(x =>
//{
//    x.SetKebabCaseEndpointNameFormatter();

//    x.UsingRabbitMq((context, cfg) =>
//    {
//        var configuration = context.GetRequiredService<IConfiguration>();
//        var connection = configuration.GetConnectionString("havensread-rabbitmq");
//        cfg.Host(connection);
//        cfg.ConfigureEndpoints(context);
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<WorkerHub>("/workerHub");

app.Run();
