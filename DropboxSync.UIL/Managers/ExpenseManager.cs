using AutoMapper;
using DropboxSync.BLL;
using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using DropboxSync.Helpers;
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
        private readonly IUploadService _uploadService;

        public ExpenseManager(ILogger<ExpenseManager> logger, IMapper mapper, IExpenseService expenseService, IFileService fileService,
            IDropboxService dropboxService, IUploadService uploadService)
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
            _uploadService = uploadService ??
                throw new ArgumentNullException(nameof(uploadService));
        }

        /// <summary>
        /// Save uploads received from the event to Dropbox as unprocessed expenses then save it locally and finally create row
        /// in the database containing event's informations
        /// </summary>
        /// <typeparam name="T">Type <see cref="ExpenseReceivedModel"/></typeparam>
        /// <param name="model">Received event's model of type <see cref="ExpenseReceivedModel"/></param>
        /// <returns><c>true</c> If every step completed successfully. <c>false</c> Otherwise</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Create<T>(T model) where T : ExpenseReceivedModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            ExpenseEntity expenseEntity = _mapper.Map<ExpenseEntity>(model);

            if (expenseEntity is null)
            {
                _logger.LogError("{date} | The mapping from {modelType} to {entityType} couldn't be made!",
                    DateTime.Now, typeof(ExpenseReceivedModel), typeof(ExpenseEntity));
                return false;
            }

            foreach (string uploadId in model.UploadIds)
            {
                SavedFile? savedFile = AsyncHelper.RunSync(() => _fileService.DownloadFile(uploadId));
                if (savedFile is null)
                {
                    _logger.LogError("{date} | File with ID \"{fileId}\" couldn't be downloaded!", DateTime.Now, uploadId);
                    return false;
                }

                DropboxSavedFile? dropboxSavedFile = AsyncHelper.RunSync(() =>
                    _dropboxService
                        .SaveUnprocessedFileAsync(
                            fileName: savedFile.FileName,
                            createdAt: DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                            fileRelativePath: savedFile.RelativePath,
                            fileType: FileTypes.Expenses));

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

        public bool Delete<T>(T entity) where T : ExpenseRemovedModel
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            foreach (string uploadId in entity.UploadIds)
            {
                if (string.IsNullOrEmpty(uploadId))
                {
                    _logger.LogWarning("{date} | An ID provided in the Expense event is null or empty.", DateTime.Now);
                    continue;
                }

                UploadEntity? uploadFromRepo = _uploadService.GetByUploadId(uploadId);

                if (uploadFromRepo is null)
                {
                    _logger.LogError("{date} | No upload with ID : \"{id}\" is registered in the database.", DateTime.Now, uploadId);
                    continue;
                }

                bool dropboxResult = AsyncHelper.RunSync(() => _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId));
                if (!dropboxResult)
                {
                    _logger.LogError("{date} | The file with ID \"{id}\"couldn't be deleted from Dropbox.", DateTime.Now, uploadId);
                    continue;
                }

                string localFileName = $"{uploadFromRepo.UploadId}-{uploadFromRepo.OriginalFileName}";
                bool localResult = _fileService.Delete(localFileName);
                if (!localResult)
                {
                    _logger.LogError("{date} | File with ID \"{id}\" couldn't be deleted locally", DateTime.Now, uploadId);
                    continue;
                }

                _uploadService.Delete(uploadFromRepo);
            }

            if (!_uploadService.SaveChanges())
            {
                _logger.LogError("{date} | Uploads couldn't be deleted from the database", DateTime.Now);
                return false;
            }

            ExpenseEntity? expenseFromRepo = _expenseService.GetById(Guid.Parse(entity.ExpenseId));
            if (expenseFromRepo is null)
            {
                _logger.LogError("{date} | Could not find any expense in the database with ID \"{id}\"", DateTime.Now, entity.ExpenseId);
                return false;
            }

            _expenseService.Delete(expenseFromRepo);

            return _expenseService.SaveChanges();
        }

        public bool RemoveExpenseAttachment(ExpenseAttachmentRemovedModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            UploadEntity? uploadFromRepo = _uploadService.GetByUploadId(model.UploadId);
            if (uploadFromRepo is null)
            {
                _logger.LogError("{date} | Could not find any upload in the database with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

            bool dropboxResult = AsyncHelper.RunSync(() => _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId));
            if (!dropboxResult)
            {
                _logger.LogError("{date} | The file with ID \"{id}\"couldn't be deleted from Dropbox.", DateTime.Now, model.UploadId);
                return false;
            }

            string localFileName = $"{uploadFromRepo.UploadId}-{uploadFromRepo.OriginalFileName}";
            bool localResult = _fileService.Delete(localFileName);
            if (!localResult)
            {
                _logger.LogError("{date} | File with ID \"{id}\" couldn't be deleted locally", DateTime.Now, model.UploadId);
                return false;
            }

            _uploadService.Delete(uploadFromRepo);

            return _uploadService.SaveChanges();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool UpdateLabel(ExpenseLabelUpdatedModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            ExpenseEntity? expenseFromRepo = _expenseService.GetById(Guid.Parse(model.ExpenseId));
            if (expenseFromRepo is null)
            {
                _logger.LogError("{date} | No expense with ID \"{id}\" was found in the database!", DateTime.Now, model.ExpenseId);
                return false;
            }

            // TODO : Move the expense to the new folder based on the label

            expenseFromRepo.Label = model.Label;
            expenseFromRepo.Price = model.PriceHVat;
            expenseFromRepo.Vat = model.Vat;
            expenseFromRepo.UpdatedAt = new DateTime(model.Timestamp);

            _expenseService.Update(expenseFromRepo);

            return _expenseService.SaveChanges();
        }

        public bool UpdatePrice(ExpensePriceUpdatedModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            ExpenseEntity? expenseFromRepo = _expenseService.GetById(Guid.Parse(model.ExpenseId));
            if (expenseFromRepo is null)
            {
                _logger.LogError("{date} | No expense with ID \"{id}\" was found in the database!", DateTime.Now, model.ExpenseId);
                return false;
            }

            expenseFromRepo.Price = model.PriceHvat;
            expenseFromRepo.Vat = model.Vat;
            expenseFromRepo.UpdatedAt = new DateTime(model.Timestamp);

            _expenseService.Update(expenseFromRepo);

            return _expenseService.SaveChanges();
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
