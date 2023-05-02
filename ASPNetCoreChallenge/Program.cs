using ASPNetCoreChallenge.Modules;

var builder = WebApplication.CreateBuilder(args);
//Add console logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();
//App API Redirect Service
builder.Services.AddRedirectAPIService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable api routing and begin cache service.
app.UseAPIRouting();

//UseRouting is after UseAPIRouting, so Routing occurs second
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();