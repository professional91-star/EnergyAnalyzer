using EnergyAnalyzer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class EnergyDataBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<EnergyHub> _hubContext;
        private readonly ILogger<EnergyDataBackgroundService> _logger;

        public EnergyDataBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<EnergyHub> hubContext,
            ILogger<EnergyDataBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Enerji veri servisi başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IModbusService>();
                    
                    var data = await modbusService.ReadEnergyDataAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Veri okuma/gönderme hatası");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
