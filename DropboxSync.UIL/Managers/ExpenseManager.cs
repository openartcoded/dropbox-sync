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

        public bool Create<T>(T entity) where T : ExpenseReceivedModel
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        bool IEventManager.Create<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
