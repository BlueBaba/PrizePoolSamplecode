using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.HostedServices
{
    public class BusService :
      IHostedService
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<BusService> _logger;
        public BusService(IBusControl busControl, ILogger<BusService> logger)
        {
            _busControl = busControl;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bus Started");
            await
               _busControl.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bus Stoped");
            return _busControl.StopAsync(cancellationToken);
        }
    }
}
