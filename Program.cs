using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var inicializar = args.Contains("--inicializar-tienda", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(a => a != "--inicializar-tienda").ToArray());
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL")));
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<SesionCuenta>();
builder.Services.AddScoped<CarritoTienda>();
builder.Services.AddScoped<PedidosTienda>();
builder.Services.AddScoped<InicializadorTienda>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(4);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Login/Login";
    options.AccessDeniedPath = "/Login/Denegado";
    options.Cookie.Name = "ChachaWillka.Acceso";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.EventsType = typeof(SesionCuenta);
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("acceso", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 12, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});

var app = builder.Build();

if (inicializar)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<InicializadorTienda>().EjecutarAsync();
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

var culturaTienda = new RequestLocalizationOptions().SetDefaultCulture("es-PE")
    .AddSupportedCultures("es-PE").AddSupportedUICultures("es-PE");
culturaTienda.RequestCultureProviders.Clear();
app.UseRequestLocalization(culturaTienda);
app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}");
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
await app.RunAsync();
