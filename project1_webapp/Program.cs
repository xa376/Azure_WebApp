using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// key vault settings
var keyVaultUri = new Uri("https://projectkeysxh1.vault.azure.net");
var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

// retrieves the vision api key from keyvault
var secretResponse = await secretClient.GetSecretAsync("vision");
var apiKey = secretResponse.Value.Value;

// sets configuration for vision service
builder.Configuration["AIKey"] = apiKey;
builder.Configuration["AIEndpoint"] = "https://multiaiservices-xh1.cognitiveservices.azure.com";

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();

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
