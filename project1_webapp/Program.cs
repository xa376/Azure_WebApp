using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// key vault settings
//var keyVaultUri = new Uri("https://projectkeysxh1.vault.azure.net");
//var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

//// retrieves the vision api key from keyvault
//var secretResponse = await secretClient.GetSecretAsync("vision");
//var apiKey = secretResponse.Value.Value;

// sets configuration for vision service
//builder.Configuration["AIKey"] = "";
//builder.Configuration["AIEndpoint"] = "";

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(),true);

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

app.Run();
