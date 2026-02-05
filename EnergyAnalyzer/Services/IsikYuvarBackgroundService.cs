using EnergyAnalyzer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class IsikYuvarBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<IsikYuvarHub> _hubContext;
        private readonly ILogger<IsikYuvarBackgroundService> _logger;

        public IsikYuvarBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<IsikYuvarHub> hubContext,
            ILogger<IsikYuvarBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ISIKYUVAR enerji veri servisi başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IIsikYuvarModbusService>();
                    
                    var data = await modbusService.ReadEnergyDataAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ISIKYUVAR veri okuma/gönderme hatası");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
