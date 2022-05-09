using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class ShutdownManager : IHostedService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public bool PleaseStop { get; private set; }
        public Task? BackgroundTask { get; private set; }


        public ShutdownManager(ILogger<ShutdownManager> logger, IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            _applicationLifetime = applicationLifetime ??
                throw new ArgumentNullException(nameof(applicationLifetime));

            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting {class}", nameof(ShutdownManager));

            BackgroundTask = Task.Run(async () =>
            {
                while (!PleaseStop)
                {
                    await Task.Delay(50);
                }

                _logger.LogInformation("Background task successfully stopped!");
            });

            if (_serviceProvider is null) throw new NullReferenceException(nameof(_serviceProvider));

            object? brokerEventListenerFromServices = _serviceProvider.GetService(typeof(BrokerEventListener))
                ?? throw new NullReferenceException(nameof(BrokerEventListener));

            BrokerEventListener? brokerEventListener = brokerEventListenerFromServices as BrokerEventListener;

            if (brokerEventListener is null) throw new NullReferenceException(nameof(brokerEventListener));

            brokerEventListener.Initialize();
            brokerEventListener.Start();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (BackgroundTask is null) throw new NullReferenceException(nameof(BackgroundTask));

            _logger.LogInformation("Service stopping...");

            PleaseStop = true;
            await BackgroundTask;

            _logger.LogInformation("Service succesfully stopped!");
        }
    }
}
