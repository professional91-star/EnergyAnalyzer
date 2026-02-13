using EnergyAnalyzer.Hubs;
using EnergyAnalyzer.Models;
using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Services
{
    public class HamalBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<HamalHub> _hubContext;
        private readonly ILogger<HamalBackgroundService> _logger;
        private readonly HamalSettings _settings;

        public HamalBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<HamalHub> hubContext,
            ILogger<HamalBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
            _settings = new HamalSettings();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HAMAL GES Background Service başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var modbusService = scope.ServiceProvider.GetRequiredService<IHamalModbusService>();

                    var data = await modbusService.ReadEnergyDataAsync();

                    await _hubContext.Clients.All.SendAsync("ReceiveEnergyData", data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "HAMAL veri okuma hatası");
                }

                await Task.Delay(_settings.PollingIntervalMs, stoppingToken);
            }
        }
    }
}
