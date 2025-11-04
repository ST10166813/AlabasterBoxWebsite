using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Alabaster.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register your custom Firebase authentication service
builder.Services.AddSingleton<FirebaseAuthService>();

// Enable session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Initialize Firebase Admin SDK once
// if (FirebaseApp.DefaultInstance == null)
// {
//     FirebaseApp.Create(new AppOptions
//     {
//         Credential = GoogleCredential.FromFile("serviceAccountKey.json")
//     });
// }

//added -> for render
// ---- Firebase Admin: env-first, then file fallback ----
if (FirebaseApp.DefaultInstance == null)
{
    // Option B: JSON in env var
    var json = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

    // Option A: Secret File path set in GOOGLE_APPLICATION_CREDENTIALS
    var path = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

    GoogleCredential creds = null!;
    if (!string.IsNullOrWhiteSpace(json))
        creds = GoogleCredential.FromJson(json);
    else if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        creds = GoogleCredential.FromFile(path);
    else if (File.Exists("serviceAccountKey.json")) // local dev fallback
        creds = GoogleCredential.FromFile("serviceAccountKey.json");
    else
        throw new InvalidOperationException("Firebase credentials not found. Set FIREBASE_CREDENTIALS_JSON or GOOGLE_APPLICATION_CREDENTIALS, or include serviceAccountKey.json locally.");

    FirebaseApp.Create(new AppOptions { Credential = creds });
}

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session and authorization
app.UseSession();
app.UseAuthorization();

// Map default controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
