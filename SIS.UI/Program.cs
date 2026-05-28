using FluentValidation.AspNetCore;
using SIS.Common.Time;
using SIS.UI.Extensions;
using SIS.UI.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuth();
builder.Services.AddApiClients(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();

// Global simulasiya vəziyyəti — singleton, bütün istifadəçilərə tətbiq edilir
builder.Services.AddSingleton<GlobalSimulationState>();
builder.Services.AddScoped<IClock, GlobalClock>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.None
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
