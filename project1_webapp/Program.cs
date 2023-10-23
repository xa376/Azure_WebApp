using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Reflection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(),true);

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "project1cache";
});

builder.Services.AddSession(options => {
    options.Cookie.Name = "AIEngineerSiteSession";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// builder.Configuration["RedisConnection"]
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseSession();

app.Run();
