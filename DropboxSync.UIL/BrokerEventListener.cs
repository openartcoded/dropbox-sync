using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using DropboxSync.BLL.IServices;
using DropboxSync.UIL.Enums;
using DropboxSync.UIL.Helpers;
using DropboxSync.UIL.Managers;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DropboxSync.UIL
{
    public class BrokerEventListener
    {
        public const int SUPPORT_EVENT_VERSION = 1;

        private readonly ILogger _logger;
        private readonly AmqpCredentialModel _amqpCredentials;
        private readonly IExpenseManager _expenseManager;
        private readonly IInvoiceManager _invoiceManager;
        private readonly IDossierManager _dossierManager;

        private readonly IDropboxService _dropboxService;

        public Connection? AmqpConnection { get; private set; }

        public BrokerEventListener(ILogger<BrokerEventListener> logger, IExpenseManager expenseManager,
            IInvoiceManager invoiceManager, IDossierManager dossierManager, IDropboxService dropboxService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _amqpCredentials = new AmqpCredentialModel() ??
                throw new NullReferenceException(nameof(AmqpCredentialModel));
            _expenseManager = expenseManager ??
                throw new ArgumentNullException(nameof(expenseManager));
            _invoiceManager = invoiceManager ??
                throw new ArgumentNullException(nameof(invoiceManager));
            _dossierManager = dossierManager
                ?? throw new ArgumentNullException(nameof(dossierManager));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
        }

        public void Initialize()
        {
            string username = _amqpCredentials.AmqpUsername;
            string password = _amqpCredentials.AmqpPassword;
            string host = _amqpCredentials.AmqpHost;
            int port = _amqpCredentials.AmqpPort;

            _logger.LogInformation("The AMQP connection to host \"{host}\" on port \"{port}\" has been established!",
                host, port);

            Address address = new Address($"amqp://{username}:{password}@{host}:{port}");
            AmqpConnection = new Connection(address);

            _logger.LogInformation("AMQP Connection established!");
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
            _logger.LogCritical("Connection to the broker closed!");
        }

        private void Message_Received(IReceiverLink receiver, Message message)
        {
            string textMessage = Encoding.UTF8.GetString((byte[])message.Body);

            if (string.IsNullOrEmpty(textMessage)) throw new NullReferenceException(nameof(textMessage));

            EventModel eventModel = JsonConvert.DeserializeObject<EventModel>(textMessage) ??
                throw new NullReferenceException(nameof(EventModel));

            BrokerEvent brokerEvent = (BrokerEvent)Enum.Parse(typeof(BrokerEvent), eventModel.EventName);
            _logger.LogInformation("{date} | Event \"{brokerEvent}\" received", DateTime.Now, brokerEvent);

            int eventVersion = StringHelper.KeepOnlyDigits(eventModel.Version);

            if (eventVersion != SUPPORT_EVENT_VERSION)
            {
                _logger.LogWarning("The event \"{eventName}\" with version \"{eventVersion}\" is not supported by " +
                    "this app (supported version: {supportedVersion})", eventModel.EventName, eventVersion, SUPPORT_EVENT_VERSION);
            }
            else
            {
                EventRedirection(brokerEvent, textMessage);
                _logger.LogInformation("{date} | Event {eventName} treated with success!", DateTime.Now, eventModel.EventName);
            }
        }

        private bool EventRedirection(BrokerEvent brokerEvent, string jsonObj)
        {
            if (jsonObj is null) throw new ArgumentNullException(nameof(jsonObj));

            switch (brokerEvent)
            {
                // Redirect all expense events with _expenseManager.Redirect()
                case BrokerEvent.ExpenseReceived:
                    ExpenseReceivedModel? expense = JsonConvert.DeserializeObject<ExpenseReceivedModel>(jsonObj);
                    if (expense is null)
                    {
                        _logger.LogError("{date} | Json couldn't be deserialized to type {type}", DateTime.Now, typeof(ExpenseReceivedModel));
                        return false;
                    }

                    return _expenseManager.Create(expense);

                case BrokerEvent.ExpenseLabelUpdated:

                    ExpenseLabelUpdatedModel? expenseLabelUpdated = JsonConvert.DeserializeObject<ExpenseLabelUpdatedModel>(jsonObj);
                    if (expenseLabelUpdated is null)
                    {
                        _logger.LogError("{date} | JSON couldn't be deserialized to type {type}",
                            DateTime.Now, typeof(ExpenseLabelUpdatedModel));

                        return false;
                    }

                    return _expenseManager.UpdateLabel(expenseLabelUpdated);

                case BrokerEvent.ExpensePriceUpdated:

                    ExpensePriceUpdatedModel? expensePriceUpdated = JsonConvert.DeserializeObject<ExpensePriceUpdatedModel>(jsonObj);
                    if (expensePriceUpdated is null)
                    {
                        _logger.LogError("{date} | JSON couldn't be deserialized to type {type}",
                            DateTime.Now, typeof(ExpensePriceUpdatedModel));
                        return false;
                    }

                    return _expenseManager.UpdatePrice(expensePriceUpdated);

                case BrokerEvent.ExpenseRemoved:

                    ExpenseRemovedModel? expenseRemoved = JsonConvert.DeserializeObject<ExpenseRemovedModel>(jsonObj);
                    if (expenseRemoved is null)
                    {
                        _logger.LogError("{date} | Json couldn't be deserialized to type {type}"
                            , DateTime.Now, typeof(ExpenseRemovedModel));
                        return false;
                    }

                    return _expenseManager.Delete(expenseRemoved);

                case BrokerEvent.ExpenseAttachmentRemoved:

                    ExpenseAttachmentRemovedModel? expenseAttachmentRemoved = JsonConvert.DeserializeObject<ExpenseAttachmentRemovedModel>(jsonObj);
                    if (expenseAttachmentRemoved is null)
                    {
                        _logger.LogError("{date} | Json couldn't be deserialized to type {type}",
                            DateTime.Now, typeof(ExpenseAttachmentRemovedModel));
                        return false;
                    }

                    return _expenseManager.RemoveExpenseAttachment(expenseAttachmentRemoved);

                case BrokerEvent.ExpensesAddedToDossier:

                    DossierExpensesAddedModel? expensesAddedModel = JsonConvert.DeserializeObject<DossierExpensesAddedModel>(jsonObj);
                    if (expensesAddedModel is null)
                    {
                        _logger.LogError("{date} | Json couldn't be deserialized to type {type}",
                            DateTime.Now, typeof(DossierExpensesAddedModel));
                        return false;
                    }
                    return false;

                case BrokerEvent.ExpenseRemovedFromDossier:

                    DossierExpenseRemovedModel? dossierExpenseRemoved = JsonConvert.DeserializeObject<DossierExpenseRemovedModel>(jsonObj);
                    if (dossierExpenseRemoved is null)
                    {
                        _logger.LogError("{date} | Json couldn't be deserialized to type {type}",
                            DateTime.Now, typeof(DossierExpenseRemovedModel));
                        return false;
                    }
                    return false;

                case BrokerEvent.InvoiceGenerated:

                    InvoiceGeneratedModel? invoiceGenerated = JsonConvert.DeserializeObject<InvoiceGeneratedModel>(jsonObj);
                    if (invoiceGenerated is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null!",
                            DateTime.Now, typeof(InvoiceGeneratedModel));
                        return false;
                    }

                    return _invoiceManager.Create(invoiceGenerated);

                case BrokerEvent.InvoiceRemoved:

                    InvoiceRemovedModel? invoiceRemoved = JsonConvert.DeserializeObject<InvoiceRemovedModel>(jsonObj);
                    if (invoiceRemoved is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(InvoiceRemovedModel));
                        return false;
                    }

                    return _invoiceManager.Delete(invoiceRemoved);

                case BrokerEvent.InvoiceRestored:

                    InvoiceRestoredModel? invoiceRestored = JsonConvert.DeserializeObject<InvoiceRestoredModel>(jsonObj);

                    if (invoiceRestored is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(InvoiceRestoredModel));
                        return false;
                    }

                    return _invoiceManager.Restore(invoiceRestored);

                case BrokerEvent.InvoiceRemovedFromDossier:
                case BrokerEvent.InvoiceAddedToDossier:
                // Redirect all dossier events with _dossierManager.Redirect()
                case BrokerEvent.DossierCreated:

                    DossierCreateModel? dossierCreate = JsonConvert.DeserializeObject<DossierCreateModel>(jsonObj);
                    if (dossierCreate is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(DossierCreateModel));
                        return false;
                    }

                    return _dossierManager.Create(dossierCreate);

                case BrokerEvent.DossierClosed:

                    DossierCloseModel? dossierClose = JsonConvert.DeserializeObject<DossierCloseModel>(jsonObj);
                    if (dossierClose is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(DossierCloseModel));
                        return false;
                    }

                    return _dossierManager.CloseDossier(dossierClose);

                case BrokerEvent.DossierDeleted:

                    DossierDeleteModel? dossierDelete = JsonConvert.DeserializeObject<DossierDeleteModel>(jsonObj);
                    if (dossierDelete is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(DossierDeleteModel));
                        return false;
                    }

                    return _dossierManager.Delete(dossierDelete);

                case BrokerEvent.DossierUpdated:

                    DossierUpdateModel? dossierUpdate = JsonConvert.DeserializeObject<DossierUpdateModel>(jsonObj);
                    if (dossierUpdate is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null", DateTime.Now, typeof(DossierUpdateModel));
                        return false;
                    }

                    return _dossierManager.Update(dossierUpdate);

                case BrokerEvent.DossierRecallForModification:

                    DossierRecallForModificationModel? dossierRecallForModification =
                        JsonConvert.DeserializeObject<DossierRecallForModificationModel>(jsonObj);

                    if (dossierRecallForModification is null)
                    {
                        _logger.LogError("{date} | The deserialized object of type {type} is null",
                            DateTime.Now, typeof(DossierRecallForModificationModel));
                        return false;
                    }

                    return _dossierManager.Recall(dossierRecallForModification);

                default:
                    _logger.LogError("Event category couldn't be defined! RECEIVED EVENT : \"{brokerEvent}\"", brokerEvent);
                    return false;
            }
        }
    }
}
