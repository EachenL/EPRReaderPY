using EasyPathology.Core.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    Args = args,
    ContentRootPath = Path.GetDirectoryName(Environment.ProcessPath)
});
builder.Services.AddGrpc();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDefaultLoggerService();
// builder.Services.AddSingleton<IConfigureService, RegistryConfigureService>();

var app = builder.Build();

app.Run();