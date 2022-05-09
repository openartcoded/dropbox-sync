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

        private bool EventRedirection(BrokerEvent brokerEvent, string jsonObj)
        {
            if (jsonObj is null) throw new ArgumentNullException(nameof(jsonObj));

            switch (brokerEvent)
            {
                // Redirect all expense events with _expenseManager.Redirect()
                case BrokerEvent.ExpenseReceived:
                case BrokerEvent.ExpenseLabelUpdated:
                case BrokerEvent.ExpensePriceUpdated:
                case BrokerEvent.ExpenseRemoved:
                case BrokerEvent.ExpenseAttachmentRemoved:
                case BrokerEvent.ExpenseAddedToDossier:
                case BrokerEvent.ExpenseRemovedFromDossier: return _expenseManager.Redirect(jsonObj);
                // Redirect all Invoice events with _invoiceManager.Redirect()
                case BrokerEvent.InvoiceGenerated:
                case BrokerEvent.InvoiceRemoved:
                case BrokerEvent.InvoiceRestored:
                case BrokerEvent.InvoiceRemovedFromDossier:
                case BrokerEvent.InvoiceAddedToDossier: return _invoiceManager.Redirect(jsonObj);
                // Redirect all dossier events with _dossierManager.Redirect()
                case BrokerEvent.DossierCreated:
                case BrokerEvent.DossierClosed:
                case BrokerEvent.DossierDeleted:
                case BrokerEvent.DossierUpdated:
                case BrokerEvent.DossierRecallForModification: return _dossierManager.Redirect(jsonObj);
                // Send a message to the log and return false
                default:
                    Display.Error($"Event couldn't be chosen!");
                    return false;
            }
        }
    }
}
