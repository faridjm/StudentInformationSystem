using System.Globalization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using SIS.Common.Time;
using SIS.UI.Extensions;
using SIS.UI.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
builder.Services.AddAuth();
builder.Services.AddApiClients(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("az-Latn-AZ"),
        new CultureInfo("en-US"),
        new CultureInfo("ru-RU")
    };
    options.DefaultRequestCulture = new RequestCulture("az-Latn-AZ");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = ".AspNetCore.Culture"
    });

    // Brauzerin Accept-Language başlığını nəzərə alma — cookie yoxdursa həmişə
    // default mədəniyyət (az-Latn-AZ) işləsin, brauzer dili üstələməsin.
    var acceptLanguageProvider = options.RequestCultureProviders
        .OfType<AcceptLanguageHeaderRequestCultureProvider>()
        .FirstOrDefault();
    if (acceptLanguageProvider is not null)
    {
        options.RequestCultureProviders.Remove(acceptLanguageProvider);
    }
});

// Global simulasiya vəziyyəti — singleton, bütün istifadəçilərə tətbiq edilir
builder.Services.AddSingleton<GlobalSimulationState>();
builder.Services.AddScoped<IClock, GlobalClock>();
builder.Services.AddScoped<SIS.UI.Services.IPublicWebsiteService, SIS.UI.Services.PublicWebsiteService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
