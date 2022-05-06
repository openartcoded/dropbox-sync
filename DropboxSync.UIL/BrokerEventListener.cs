using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using DropboxSync.UIL.Enums;
using DropboxSync.UIL.Helpers;
using DropboxSync.UIL.Managers;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DropboxSync.UIL
{
    public class BrokerEventListener
    {
        public const int SUPPORT_EVENT_VERSION = 1;

        private readonly AmqpCredentialModel _amqpCredentials;
        private readonly IExpenseManager _expenseManager;
        public Connection? AmqpConnection { get; private set; }

        public BrokerEventListener(IExpenseManager expenseManager)
        {
            _amqpCredentials = new AmqpCredentialModel() ??
                throw new NullReferenceException(nameof(AmqpCredentialModel));
            _expenseManager = expenseManager ??
                throw new ArgumentNullException(nameof(expenseManager));
        }

        public void Initialize()
        {
            string username = _amqpCredentials.AmqpUsername;
            string password = _amqpCredentials.AmqpPassword;
            string host = _amqpCredentials.AmqpHost;
            int port = _amqpCredentials.AmqpPort;

            Display.News($"AMQP Connection to host \"{host}\" on port \"{port}\"");

            Address address = new Address($"amqp://{username}:{password}@{host}:{port}");
            AmqpConnection = new Connection(address);

            Display.News($"AMQP Connection established!");
            AmqpConnection.Closed += Connection_Closed;
        }

        public void Start()
        {
            if (AmqpConnection is null) throw new NullReferenceException(nameof(AmqpConnection));

            Session session = new Session(AmqpConnection);

            ReceiverLink receiverLink = new ReceiverLink(session, "", _amqpCredentials.AmqpQueue);
            receiverLink.Start(200, Message_Received);
        }

        private void Connection_Closed(IAmqpObject sender, Amqp.Framing.Error error)
        {
            Display.Error("Connection to the broker closed!");
        }

        private void Message_Received(IReceiverLink receiver, Message message)
        {
            string textMessage = Encoding.UTF8.GetString((byte[])message.Body);

            JObject jobject = JObject.Parse(textMessage);
            JToken eventToken = jobject["eventName"] ??
                throw new NullReferenceException($"Event name could not be found!");
            JToken versionToken = jobject["version"] ??
                throw new NullReferenceException($"Event version could not be found!");

            string eventName = eventToken.ToString() ?? throw new NullReferenceException(nameof(eventName));
            int version = StringHelper.KeepOnlyDigits(versionToken.ToString());

            BrokerEvent brokerEvent = (BrokerEvent)Enum.Parse(typeof(BrokerEvent), eventName);
            Display.News($"Event \\{brokerEvent}\\ received!");

            if (version != SUPPORT_EVENT_VERSION)
            {
                Display.Log($"The event \\{eventName}\\ with version \\{version}\\ is not supported by this app " +
                    $"(supported version :{SUPPORT_EVENT_VERSION})");
            }
            else
            {
                EventRedirection(brokerEvent, textMessage);
            }
        }

        private void EventRedirection(BrokerEvent brokerEvent, string jsonObj)
        {
            if (jsonObj is null) throw new ArgumentNullException(nameof(jsonObj));

            switch (brokerEvent)
            {
                case BrokerEvent.ExpenseReceived:

                    ExpenseReceivedModel expenseReceived = JsonConvert.DeserializeObject<ExpenseReceivedModel>(jsonObj)
                        ?? throw new NullReferenceException(nameof(expenseReceived));

                    break;
                case BrokerEvent.ExpenseLabelUpdated:

                    ExpenseLabelUpdateModel expenseLabelUpdate = JsonConvert.DeserializeObject<ExpenseLabelUpdateModel>(jsonObj)
                        ?? throw new NullReferenceException(nameof(expenseLabelUpdate));

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
