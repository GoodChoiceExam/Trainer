namespace FitLife.Trainer.Api.Services;

// BackgroundService der logger et heartbeat hvert 60. sekund.
// Bruges af Grafana-dashboardet til at vise om servicen er online.
public class HeartbeatService(ILogger<HeartbeatService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Heartbeat");
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
