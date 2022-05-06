using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using DropboxSync.UIL.Enums;
using Newtonsoft.Json.Linq;

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

            JObject jobject = JObject.Parse(textMessage);
            JToken token = jobject["eventName"] ??
                throw new NullReferenceException($"Event name could not be foudn!");

            string eventName = token.ToString();
            BrokerEvent brokerEvent = (BrokerEvent)Enum.Parse(typeof(BrokerEvent), eventName);

            Display.News($"Event \\{brokerEvent}\\ was received!");

            EventRedirection(brokerEvent);
        }

        private void EventRedirection(BrokerEvent brokerEvent)
        {
            switch (brokerEvent)
            {
                case BrokerEvent.ExpenseReceived:
                    break;
                case BrokerEvent.ExpenseLabelUpdated:
                    break;
                case BrokerEvent.ExpensePriceUpdated:
                    break;
                case BrokerEvent.ExpenseRemoved:
                    break;
                case BrokerEvent.ExpenseAttachmentRemoved:
                    break;
                case BrokerEvent.InvoiceGenerated:
                    break;
                case BrokerEvent.InvoiceRemoved:
                    break;
                case BrokerEvent.InvoiceRestored:
                    break;
                case BrokerEvent.DossierCreated:
                    break;
                case BrokerEvent.ExpenseAddedToDossier:
                    break;
                case BrokerEvent.ExpenseRemovedFromDossier:
                    break;
                case BrokerEvent.InvoiceAddedToDossier:
                    break;
                case BrokerEvent.InvoiceRemovedFromDossier:
                    break;
                case BrokerEvent.DossierClosed:
                    break;
                case BrokerEvent.DossierDeleted:
                    break;
                case BrokerEvent.DossierUpdated:
                    break;
                case BrokerEvent.DossierRecallForModification:
                    break;
                default:
                    Display.Error($"Event couldn't be chosen!");
                    break;
            }
        }
    }
}
