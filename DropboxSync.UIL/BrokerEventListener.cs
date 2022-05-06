using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;

namespace DropboxSync.UIL
{
    public class BrokerEventListener
    {
        private readonly Connection _connection;
        private readonly string _queue;

        public BrokerEventListener(string username, string password, string host, int port, string queue)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));
            if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));

            _queue = queue;

            Address address = new Address($"amqp://{username}:{password}@{host}:{port}");
            _connection = new Connection(address);
            _connection.Closed += Connection_Closed;
        }

        private void Connection_Closed(IAmqpObject sender, Amqp.Framing.Error error)
        {
            Display.Error("Connection to the broker closed!");
        }

        public void Start()
        {
            Session session = new Session(_connection);

            ReceiverLink receiverLink = new ReceiverLink(session, "", _queue);
            receiverLink.Start(200, Message_Received);
        }

        private void Message_Received(IReceiverLink receiver, Message message)
        {
            string textMessage = Encoding.UTF8.GetString((byte[])message.Body);
            Console.WriteLine(textMessage);
        }
    }
}
