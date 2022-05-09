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
        private readonly IInvoiceManager _invoiceManager;
        private readonly IDossierManager _dossierManager;
        public Connection? AmqpConnection { get; private set; }

        public BrokerEventListener(IExpenseManager expenseManager, IInvoiceManager invoiceManager, IDossierManager dossierManager)
        {
            _amqpCredentials = new AmqpCredentialModel() ??
                throw new NullReferenceException(nameof(AmqpCredentialModel));
            _expenseManager = expenseManager ??
                throw new ArgumentNullException(nameof(expenseManager));
            _invoiceManager = invoiceManager ??
                throw new ArgumentNullException(nameof(invoiceManager));
            _dossierManager = dossierManager
                ?? throw new ArgumentNullException(nameof(dossierManager));
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

            if (string.IsNullOrEmpty(textMessage)) throw new NullReferenceException(nameof(textMessage));

            EventModel eventModel = JsonConvert.DeserializeObject<EventModel>(textMessage) ??
                throw new NullReferenceException(nameof(EventModel));

            BrokerEvent brokerEvent = (BrokerEvent)Enum.Parse(typeof(BrokerEvent), eventModel.EventName);
            Display.News($"Event \"{brokerEvent}\" received!");

            int eventVersion = StringHelper.KeepOnlyDigits(eventModel.Version);

            if (eventVersion != SUPPORT_EVENT_VERSION)
            {
                Display.Log($"The event \"{eventModel.EventName}\" with version \"{eventVersion}\" is not supported by this app " +
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
                case BrokerEvent.ExpenseLabelUpdated:
                case BrokerEvent.ExpensePriceUpdated:
                case BrokerEvent.ExpenseRemoved:
                case BrokerEvent.ExpenseAttachmentRemoved:
                case BrokerEvent.ExpenseAddedToDossier:
                case BrokerEvent.ExpenseRemovedFromDossier:
                    _expenseManager.Redirect(jsonObj);
                    break;
                case BrokerEvent.InvoiceGenerated:

                    break;
                case BrokerEvent.InvoiceRemoved:
                    break;
                case BrokerEvent.InvoiceRestored:
                    break;
                case BrokerEvent.DossierCreated:
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
