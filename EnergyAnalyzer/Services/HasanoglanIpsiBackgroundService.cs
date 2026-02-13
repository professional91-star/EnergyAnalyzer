using EnergyAnalyzer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class HasanoglanIpsiBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<HasanoglanIpsiHub> _hubContext;
        private readonly ILogger<HasanoglanIpsiBackgroundService> _logger;

        public HasanoglanIpsiBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<HasanoglanIpsiHub> hubContext,
            ILogger<HasanoglanIpsiBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hasanoğlan İPSİ enerji veri servisi başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IHasanoglanIpsiModbusService>();
                    
                    var data = await modbusService.ReadEnergyDataAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hasanoğlan İPSİ veri okuma/gönderme hatası");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
