using EnergyAnalyzer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class HasanoglanIsikYuvarBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<HasanoglanIsikYuvarHub> _hubContext;
        private readonly ILogger<HasanoglanIsikYuvarBackgroundService> _logger;

        public HasanoglanIsikYuvarBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<HasanoglanIsikYuvarHub> hubContext,
            ILogger<HasanoglanIsikYuvarBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hasanoğlan IŞIKYUVAR enerji veri servisi başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IHasanoglanIsikYuvarModbusService>();
                    
                    var data = await modbusService.ReadEnergyDataAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hasanoğlan IŞIKYUVAR veri okuma/gönderme hatası");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
