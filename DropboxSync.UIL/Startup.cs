using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL
{
    public static class StartUp
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        private static void ConfigureServices(IServiceCollection services)
        {
            string amqpUsername = Environment.GetEnvironmentVariable("AMQP_USERNAME") ?? "root";
            string amqpPassword = Environment.GetEnvironmentVariable("AMQP_PASSWORD") ?? "root";
            string amqpHost = Environment.GetEnvironmentVariable("AMQP_HOST") ?? "localhost";
            if (!int.TryParse(Environment.GetEnvironmentVariable($"AMQP_PORT"), out int amqpPort)) amqpPort = 61616;
            string amqpQueue = Environment.GetEnvironmentVariable("AMQP_QUEUE") ?? "backend-event";

            BrokerEventListener brokerEventListener = new BrokerEventListener(amqpUsername, amqpPassword, amqpHost, amqpPort, amqpQueue);
            if (brokerEventListener is null) throw new ArgumentNullException(nameof(brokerEventListener));
            brokerEventListener.Start();
        }

        public static void Build()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
