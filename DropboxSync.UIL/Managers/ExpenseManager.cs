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
        private readonly IDropboxService _dropboxService;

        public ExpenseManager(ILogger<ExpenseManager> logger, IExpenseService expenseService, IFileService fileService,
            IDropboxService dropboxService)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _expenseService = expenseService ??
                throw new ArgumentNullException(nameof(expenseService));
            _fileService = fileService
                ?? throw new ArgumentNullException(nameof(fileService));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
        }

        // TODO : 1. Create a local backup
        // TODO : 2. Create a dropbox backup
        // TODO : 3. Save information in database
        public bool Create<T>(T entity) where T : ExpenseReceivedModel
        {
            throw new NotImplementedException();
        }

        // TODO : 1. Delete file from local backup
        // TODO : 2. Delete file from Dropbox
        // TODO : 3. Delete rows from database
        public bool Delete<T>(T entity) where T : ExpenseRemovedModel
        {
            throw new NotImplementedException();
        }

        // TODO : 1. Delete file from local backup
        // TODO : 2. Delete file from Dropbox
        // TODO : 3. Delete upload row from database
        public bool RemoveExpenseAttachment(ExpenseAttachmentRemovedModel model)
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        // TODO : 1. Save new file over old one in local backup
        // TODO : 2. Save new file over old one in Dropbox
        // TODO : 3. Update information in the database
        public bool UpdateLabel(ExpenseLabelUpdatedModel model)
        {
            throw new NotImplementedException();
        }

        // TODO : 1. Save new file over old one in local backup
        // TODO : 2. Save new file over old one in Dropbox
        // TODO : 3. Update information in the database
        public bool UpdatePrice(ExpensePriceUpdatedModel model)
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Create<T>(T model)
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Delete<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
