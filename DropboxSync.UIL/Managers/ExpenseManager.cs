using AutoMapper;
using DropboxSync.BLL;
using DropboxSync.BLL.Entities;
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
        private readonly IMapper _mapper;
        private readonly IExpenseService _expenseService;
        private readonly IFileService _fileService;
        private readonly IDropboxService _dropboxService;

        public ExpenseManager(ILogger<ExpenseManager> logger, IMapper mapper, IExpenseService expenseService, IFileService fileService,
            IDropboxService dropboxService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _expenseService = expenseService ??
                throw new ArgumentNullException(nameof(expenseService));
            _fileService = fileService ??
                throw new ArgumentNullException(nameof(fileService));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
        }

        // TODO : 1. Create a local backup
        // TODO : 2. Create a dropbox backup
        // TODO : 3. Save information in database
        public bool Create<T>(T model) where T : ExpenseReceivedModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            // TODO : Create mapper profile from ExpenseReceivedModel to ExpenseEntity
            ExpenseEntity expenseEntity = _mapper.Map<ExpenseEntity>(model);

            if (expenseEntity is null)
            {
                _logger.LogError("{date} | The mapping from {modelType} to {entityType} couldn't be made!",
                    DateTime.Now, typeof(ExpenseReceivedModel), typeof(ExpenseEntity));
                return false;
            }

            foreach (string uploadId in model.UploadIds)
            {
                SavedFile? savedFile = Task.Run(async () => await _fileService.DownloadFile(uploadId)).Result;
                if (savedFile is null)
                {
                    _logger.LogError("{date} | File with ID \"{fileId}\" couldn't be downloaded!", DateTime.Now, uploadId);
                    return false;
                }

                DropboxSavedFile? dropboxSavedFile = Task.Run(async () => await _dropboxService.SaveUnprocessedFile(savedFile.FileName, DateTime.Now,
                    savedFile.RelativePath, FileTypes.Expenses, savedFile.FileExtension)).Result;

                if (dropboxSavedFile is null)
                {
                    _logger.LogError("{date} | File with ID : \"{fileID}\" couldn't be saved. Please read precedent logs to understand " +
                        "the why.", DateTime.Now, uploadId);
                    continue;
                }

                if (expenseEntity.Uploads is null) expenseEntity.Uploads = new List<UploadEntity>();

                UploadEntity upload = new UploadEntity()
                {
                    ContentType = savedFile.ContentType,
                    FileExtention = savedFile.FileExtension ?? "UNKNOWN",
                    FileSize = savedFile.FileSize,
                    OriginalFileName = savedFile.FileName,
                    DropboxFileId = dropboxSavedFile.DropboxFileId,
                    UploadId = uploadId,
                    Id = Guid.NewGuid(),
                };

                expenseEntity.Uploads.Add(upload);
            }

            _expenseService.Create(expenseEntity);

            if (!_expenseService.SaveChanges())
            {
                _logger.LogError("{date} | Expense with ID : \"{expenseId}\" couldn't be saved in the database.", DateTime.Now, model.ExpenseId);
                return false;
            }

            _logger.LogInformation("{date} | Expense with ID : \"{expenseId}\" was added to the database with its upload list.", DateTime.Now,
                model.ExpenseId);

            return true;
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
