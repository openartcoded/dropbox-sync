using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Models
{
    public class AmqpCredentialModel
    {
        public string AmqpUsername { get; private set; }
        public string AmqpPassword { get; private set; }
        public string AmqpHost { get; private set; }
        public int AmqpPort { get; private set; }
        public string AmqpQueue { get; private set; }

        public AmqpCredentialModel()
        {
            AmqpUsername = Environment.GetEnvironmentVariable("AMQP_USERNAME") ?? "root";
            AmqpPassword = Environment.GetEnvironmentVariable("AMQP_PASSWORD") ?? "root";
            AmqpHost = Environment.GetEnvironmentVariable("AMQP_HOST") ?? "localhost";

            if (int.TryParse(Environment.GetEnvironmentVariable("AMQP_PORT"), out int amqpPort))
            {
                AmqpPort = amqpPort;
            }
            else
            {
                AmqpPort = 61616;
            }

            AmqpQueue = Environment.GetEnvironmentVariable("AMQP_QUEUE") ?? "backend-event";
        }
    }
}
