using EasyPathology.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Server.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
	Args = args,
	ContentRootPath = Path.GetDirectoryName(Environment.ProcessPath)
});
builder.Services.Configure<ApiBehaviorOptions>(static opt => opt.SuppressModelStateInvalidFilter = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

#if DEBUG
builder.Services.AddSwaggerGen(options => {
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
#endif

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDefaultLoggerService();
// builder.Services.AddSingleton<IConfigureService, RegistryConfigureService>();

var app = builder.Build();

#if DEBUG
app.UseSwagger();
app.UseSwaggerUI();
#endif

app.UseGlobalErrorHandler();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseSession();
app.UseHsts();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(static o => {
	o.MapControllers();
});
app.Run();
