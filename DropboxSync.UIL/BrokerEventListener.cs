using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL
{
    public class BrokerEventListener
    {
        private readonly IConnection _connection;
        private readonly string _queue;

        public BrokerEventListener(string username, string password, string host, int port, string queue)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));
            if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                UserName = username,
                Password = password,
                HostName = host,
                Port = port
            };

            _queue = queue;

            try
            {
                _connection = connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e)
            {
                
            }

        }

        public void Start()
        {
            var channel = _connection.CreateModel();
            channel.QueueDeclare(_queue);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += OnQueueMessageReceived;

            channel.BasicConsume(_queue, true, consumer);
        }

        private void OnQueueMessageReceived(object? sender, BasicDeliverEventArgs e)
        {
            Console.WriteLine("Message received");
            var body = e.Body.ToArray();
            Console.WriteLine($"{Encoding.UTF8.GetString(body)}");
        }
    }
}
