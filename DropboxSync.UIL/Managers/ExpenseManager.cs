using DropboxSync.BLL.IServices;
using DropboxSync.UIL.Enums;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class ExpenseManager : IExpenseManager
    {
        private readonly ILogger _logger;
        private readonly IExpenseService _expenseService;
        private readonly IFileService _fileService;

        public ExpenseManager(ILogger<ExpenseManager> logger, IExpenseService expenseService, IFileService fileService)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _expenseService = expenseService ??
                throw new ArgumentNullException(nameof(expenseService));
            _fileService = fileService
                ?? throw new ArgumentNullException(nameof(fileService));
        }

        public bool Create(ExpenseModelBase model)
        {
            if (model.GetType() != typeof(ExpenseReceivedModel))
                throw new ArgumentException($"To create an expense, {nameof(model)}'s type must be of " +
                    $"type {typeof(ExpenseReceivedModel)}");

            if (model is null) throw new ArgumentNullException(nameof(model));

            ExpenseReceivedModel? expenseReceived = model as ExpenseReceivedModel;

            foreach (string item in expenseReceived.UploadIds)
            {
                _fileService.DownloadFile(item);
            }

            return true;
        }

        public bool Delete(ExpenseModelBase model)
        {
            throw new NotImplementedException();
        }

        public bool Redirect(string eventJson)
        {
            if (string.IsNullOrEmpty(eventJson)) throw new ArgumentNullException(nameof(eventJson));

            EventModel eventModel = JsonConvert.DeserializeObject<EventModel>(eventJson)
                ?? throw new NullReferenceException(nameof(EventModel));

            BrokerEvent brokerEvent = (BrokerEvent)Enum.Parse(typeof(BrokerEvent), eventModel.EventName);

            switch (brokerEvent)
            {
                case BrokerEvent.ExpenseReceived:
                    ExpenseReceivedModel expenseReceivedModel = JsonConvert.DeserializeObject<ExpenseReceivedModel>(eventJson)
                        ?? throw new NullReferenceException($"Json \"{eventJson}\" couldn't be parsed to type " +
                            $"\"{nameof(ExpenseReceivedModel)}\"");

                    return Create(expenseReceivedModel);

                case BrokerEvent.ExpenseLabelUpdated:
                    return false;
                case BrokerEvent.ExpensePriceUpdated:
                    return false;
                case BrokerEvent.ExpenseRemoved:
                    return false;
                case BrokerEvent.ExpenseAttachmentRemoved:
                    return false;
                case BrokerEvent.ExpenseAddedToDossier:
                    return false;
                case BrokerEvent.ExpenseRemovedFromDossier:
                    return false;
                default:
                    _logger.LogError("The right event couldn't be chosen! EVENT : [{event}]", brokerEvent);
                    return false;
            }
        }

        public bool Update(ExpenseModelBase model)
        {
            throw new NotImplementedException();
        }
    }
}
