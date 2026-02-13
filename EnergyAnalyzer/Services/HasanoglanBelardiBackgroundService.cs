using EnergyAnalyzer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class HasanoglanBelardiBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<HasanoglanBelardiHub> _hubContext;
        private readonly ILogger<HasanoglanBelardiBackgroundService> _logger;

        public HasanoglanBelardiBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<HasanoglanBelardiHub> hubContext,
            ILogger<HasanoglanBelardiBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hasanoğlan BELARDİ enerji veri servisi başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IHasanoglanBelardiModbusService>();
                    
                    var data = await modbusService.ReadEnergyDataAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hasanoğlan BELARDİ veri okuma/gönderme hatası");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
