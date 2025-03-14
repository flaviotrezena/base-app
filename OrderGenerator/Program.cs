var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<FixApplication>();
builder.Services.AddTransient<OrderService>();

var app = builder.Build();

var fixApplication = app.Services.GetRequiredService<FixApplication>();
fixApplication.Start();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

app.Lifetime.ApplicationStopping.Register(() =>
{
    fixApplication.Stop();
});
