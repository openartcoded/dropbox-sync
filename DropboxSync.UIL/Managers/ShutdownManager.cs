using Microsoft.Extensions.Hosting;
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

        public bool PleaseStop { get; private set; }
        public Task? BackgroundTask { get; private set; }


        public ShutdownManager(IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider)
        {
            _applicationLifetime = applicationLifetime ??
                throw new ArgumentNullException(nameof(applicationLifetime));

            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Starting {nameof(ShutdownManager)}");

            BackgroundTask = Task.Run(async () =>
            {
                while (!PleaseStop)
                {
                    await Task.Delay(50);
                }

                Console.WriteLine("Background task successfully stopped!");
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

            Console.WriteLine("Stopping service");

            PleaseStop = true;
            await BackgroundTask;

            Console.WriteLine("Service stopped");
        }
    }
}
