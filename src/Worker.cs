#if _WINDOWS
using WindowsSMB;
#else
using LinuxSMB;
#endif


namespace smb_healthcheck_widget;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            foreach (var share in SMBShare.Enumerate())
            {
                if (share.IsConnected())
                {
                    _logger.LogInformation($"{share.Address}/{share.Share}: ok");
                }
                else
                {
                    _logger.LogInformation($"{share.Address}/{share.Share}: {share.Diagnose()}");
                }
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}
