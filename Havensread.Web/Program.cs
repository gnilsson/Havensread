using Havensread.Connector;
using Havensread.Web.Components;
using Havensread.IngestionService.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR();

//builder.Services.AddSingleton<WorkerCoordinator>();
//builder.Services.AddSingleton<IWorkerCoordinator>(sp => sp.GetRequiredService<WorkerCoordinator>());


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

//app.MapStaticAssets();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<WorkerHub>("/workerHub");

app.Run();
