using EnergyAnalyzer.Hubs;
using EnergyAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// ISIKKURE Santral servisleri
builder.Services.AddScoped<IModbusService, ModbusService>();
builder.Services.AddHostedService<EnergyDataBackgroundService>();

// ISIKYUVAR Santral servisleri
builder.Services.AddScoped<IIsikYuvarModbusService, IsikYuvarModbusService>();
builder.Services.AddHostedService<IsikYuvarBackgroundService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<EnergyHub>("/energyhub");
app.MapHub<IsikYuvarHub>("/isikYuvarHub");

app.Run();
