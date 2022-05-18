﻿using AutoMapper;
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

                bool dropboxResult = Task.Run(async () => await _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId)).Result;
                if (!dropboxResult)
                {
                    _logger.LogError("{date} | The file with ID \"{id}\"couldn't be deleted from Dropbox.", DateTime.Now, uploadId);
                    continue;
                }

                string localFileName = $"{uploadFromRepo.Id}-{uploadFromRepo.OriginalFileName}";
                bool localResult = _fileService.Delete(localFileName);
                if (!localResult)
                {
                    _logger.LogError("{date} | File with ID \"{id}\" couldn't be deleted locally", DateTime.Now, uploadId);
                    continue;
                }

                _uploadService.Delete(uploadFromRepo);
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

        // TODO : 1. Delete file from local backup
        // TODO : 2. Delete file from Dropbox
        // TODO : 3. Delete upload row from database
        public bool RemoveExpenseAttachment(ExpenseAttachmentRemovedModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            UploadEntity? uploadFromRepo = _uploadService.GetByUploadId(model.UploadId);
            if (uploadFromRepo is null)
            {
                _logger.LogError("{date} | Could not find any upload in the database with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

            bool dropboxResult = Task.Run(async () => await _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId)).Result;
            if (!dropboxResult)
            {
                _logger.LogError("{date} | The file with ID \"{id}\"couldn't be deleted from Dropbox.", DateTime.Now, model.UploadId);
                return false;
            }

            string localFileName = $"{uploadFromRepo.Id}-{uploadFromRepo.OriginalFileName}";
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

        // TODO : 1. Save new file over old one in local backup
        // TODO : 2. Save new file over old one in Dropbox
        // TODO : 3. Update information in the database
        public bool UpdateLabel(ExpenseLabelUpdatedModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            ExpenseEntity? expenseFromRepo = _expenseService.GetById(Guid.Parse(model.ExpenseId));
            if (expenseFromRepo is null)
            {
                _logger.LogError("{date} | No expense with ID \"{id}\" was found in the database!", DateTime.Now, model.ExpenseId);
                return false;
            }

            expenseFromRepo.Label = model.Label;
            expenseFromRepo.Price = model.PriceHVat;
            expenseFromRepo.Vat = model.Vat;
            expenseFromRepo.UpdatedAt = new DateTime(model.Timestamp);

            _expenseService.Update(expenseFromRepo);

            return _expenseService.SaveChanges();
        }

        // TODO : 1. Save new file over old one in local backup
        // TODO : 2. Save new file over old one in Dropbox
        // TODO : 3. Update information in the database
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
