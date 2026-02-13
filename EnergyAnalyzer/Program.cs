using EnergyAnalyzer.Hubs;
using EnergyAnalyzer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

// ISIKKURE Santral servisleri
builder.Services.AddScoped<IModbusService, ModbusService>();
builder.Services.AddHostedService<EnergyDataBackgroundService>();

// ISIKYUVAR Santral servisleri
builder.Services.AddScoped<IIsikYuvarModbusService, IsikYuvarModbusService>();
builder.Services.AddHostedService<IsikYuvarBackgroundService>();

// HASANOGLAN Santral servisleri
builder.Services.AddScoped<IHasanoglanModbusService, HasanoglanModbusService>();
builder.Services.AddHostedService<HasanoglanBackgroundService>();

// HASANOGLAN IŞIKYUVAR Santral servisleri
builder.Services.AddScoped<IHasanoglanIsikYuvarModbusService, HasanoglanIsikYuvarModbusService>();
builder.Services.AddHostedService<HasanoglanIsikYuvarBackgroundService>();

// HASANOGLAN IŞIKKURE Santral servisleri
builder.Services.AddScoped<IHasanoglanIsikKureModbusService, HasanoglanIsikKureModbusService>();
builder.Services.AddHostedService<HasanoglanIsikKureBackgroundService>();

// HASANOGLAN BELARDİ Santral servisleri
builder.Services.AddScoped<IHasanoglanBelardiModbusService, HasanoglanBelardiModbusService>();
builder.Services.AddHostedService<HasanoglanBelardiBackgroundService>();

// HASANOGLAN İPSİ Santral servisleri
builder.Services.AddScoped<IHasanoglanIpsiModbusService, HasanoglanIpsiModbusService>();
builder.Services.AddHostedService<HasanoglanIpsiBackgroundService>();

// HAMAL GES Santral servisleri
builder.Services.AddScoped<IHamalModbusService, HamalModbusService>();
builder.Services.AddHostedService<HamalBackgroundService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.MapHub<EnergyHub>("/energyhub");
app.MapHub<IsikYuvarHub>("/isikYuvarHub");
app.MapHub<HasanoglanHub>("/hasanoglanHub");
app.MapHub<HasanoglanIsikYuvarHub>("/hasanoglanIsikYuvarHub");
app.MapHub<HasanoglanIsikKureHub>("/hasanoglanIsikKureHub");
app.MapHub<HasanoglanBelardiHub>("/hasanoglanBelardiHub");
app.MapHub<HasanoglanIpsiHub>("/hasanoglanIpsiHub");
app.MapHub<HamalHub>("/hamalHub");

app.Run();
