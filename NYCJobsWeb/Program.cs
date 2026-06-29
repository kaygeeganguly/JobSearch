using NYCJobsWeb;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

builder.Services.AddSingleton<JobsSearch>();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseStaticFiles();

foreach (var (folder, requestPath) in new[]
{
    ("Content", "/Content"),
    ("Scripts", "/Scripts"),
    ("Images", "/Images"),
    ("Fonts", "/Fonts")
})
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, folder)),
        RequestPath = requestPath
    });
}

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
