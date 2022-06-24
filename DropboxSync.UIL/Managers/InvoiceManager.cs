using System.Reflection.Metadata;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using DropboxSync.BLL;
using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using DropboxSync.Helpers;
using DropboxSync.UIL.Attributes;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropboxSync.UIL.Enums;

namespace DropboxSync.UIL.Managers
{
    internal class InvoiceManager : IInvoiceManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IInvoiceService _invoiceService;
        private readonly IUploadService _uploadService;
        private readonly IFileService _fileService;
        private readonly IDropboxService _dropboxService;

        public InvoiceManager(ILogger<InvoiceManager> logger, IMapper mapper, IInvoiceService invoiceService, IUploadService uploadService,
            IFileService fileService, IDropboxService dropboxService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _invoiceService = invoiceService ??
                throw new ArgumentNullException(nameof(invoiceService));
            _uploadService = uploadService ??
                throw new ArgumentNullException(nameof(uploadService));
            _fileService = fileService ??
                throw new ArgumentNullException(nameof(fileService));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
        }

        [MethodEvent(typeof(InvoiceGeneratedModel), nameof(BrokerEvent.InvoiceGenerated))]
        public bool Create<T>(T model) where T : InvoiceGeneratedModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            InvoiceEntity? entityToCreate = _mapper.Map<InvoiceEntity>(model);
            if (entityToCreate is null)
            {
                _logger.LogError("{date} | Couldn't map {entityName} to {entityCreate}",
                    DateTime.Now, nameof(model), nameof(entityToCreate));
                return false;
            }

            SavedFile? localSaveResult = AsyncHelper.RunSync(() => _fileService.DownloadFile(model.UploadId));
            if (localSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't save file locally", DateTime.Now);
                return false;
            }

            DropboxSavedFile? dropboxSaveResult = AsyncHelper.RunSync(() =>
                _dropboxService.SaveUnprocessedFileAsync(
                    fileName: localSaveResult.FileName,
                    createdAt: DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                    fileRelativePath: localSaveResult.RelativePath,
                    fileType: FileTypes.Invoices));

            if (dropboxSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't save file with ID \"{id}\" in Dropbox", DateTime.Now, model.UploadId);
                return false;
            }

            UploadEntity upload = new UploadEntity()
            {
                UploadId = model.UploadId,
                ContentType = localSaveResult.ContentType,
                DropboxFileId = dropboxSaveResult.DropboxFileId,
                FileSize = localSaveResult.FileSize,
                OriginalFileName = localSaveResult.FileName,
                Id = Guid.NewGuid()
            };

            entityToCreate.Upload = upload;

            _invoiceService.Create(entityToCreate);

            if (!_invoiceService.SaveChanges())
            {
                _logger.LogError("{date} | Couldn't save the Invoice in the database", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Invoice and Upload successfully saved in the database", DateTime.Now);

            return true;
        }

        public bool Delete<T>(T entity) where T : InvoiceRemovedModel
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            InvoiceEntity? entityFromRepo = _invoiceService.GetById(Guid.Parse(entity.InvoiceId));
            if (entityFromRepo is null)
            {
                _logger.LogWarning("{date} | There is no invoice in the database with ID : \"{invoiceId}\"",
                    DateTime.Now, entity.InvoiceId);
                return false;
            }

            if (entityFromRepo.UploadId is null)
            {
                _logger.LogWarning("{date} | Invoice with ID \"{id}\" in the database has no attachment!",
                    DateTime.Now, entityFromRepo.Id);
                return false;
            }

            UploadEntity? uploadFromRepo = _uploadService.GetById(entityFromRepo.UploadId.Value);

            if (uploadFromRepo is null)
            {
                _logger.LogError("{date} | Could not find any upload in the database with ID \"{id}\"",
                    DateTime.Now, entityFromRepo.UploadId.Value);
                return false;
            }

            bool dropboxDeleteResult = AsyncHelper.RunSync(() => _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId));
            if (!dropboxDeleteResult)
            {
                _logger.LogError("{date} | Could not delete file with ID : \"{id}\" from Dropbox", DateTime.Now, uploadFromRepo.UploadId);
                return false;
            }

            string localFilename = $"{uploadFromRepo.UploadId}-{uploadFromRepo.OriginalFileName}";
            bool localDeleteResult = _fileService.Delete(localFilename);
            if (!localDeleteResult)
            {
                _logger.LogError("{date} | Could not delete local file", DateTime.Now);
                return false;
            }

            _uploadService.Delete(uploadFromRepo);

            if (!_uploadService.SaveChanges())
            {
                _logger.LogError("{date} | Could not delete upload with ID \"{id}\" from the database", DateTime.Now, uploadFromRepo.Id);
                return false;
            }

            if (entity.LogicalDelete)
            {
                entityFromRepo.Deleted = true;
                _invoiceService.Update(entityFromRepo);
            }
            else
            {
                _invoiceService.Delete(entityFromRepo);
            }

            return _invoiceService.SaveChanges();
        }

        public bool Restore(InvoiceRestoredModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            InvoiceEntity? invoiceFromRepo = _invoiceService.GetById(Guid.Parse(model.InvoiceId));
            if (invoiceFromRepo is null)
            {
                _logger.LogWarning("{date} | There is no invoice with ID \"{id}\" in the database to retrieve.", DateTime.Now, model.InvoiceId);
                return false;
            }

            SavedFile? saveLocalResult = AsyncHelper.RunSync(() => _fileService.DownloadFile(model.UploadId));
            if (saveLocalResult is null)
            {
                _logger.LogError("{date} | Could not locally save upload with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

            DropboxSavedFile? dropSaveResult = AsyncHelper.RunSync(() =>
                _dropboxService.SaveUnprocessedFileAsync(
                    fileName: saveLocalResult.FileName,
                    createdAt: DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                    fileRelativePath: saveLocalResult.RelativePath,
                    fileType: FileTypes.Invoices));

            if (dropSaveResult is null)
            {
                _logger.LogError("{date} | Could not save upload with ID \"{id}\" in Dropbox", DateTime.Now, model.UploadId);
                return false;
            }

            UploadEntity upload = new UploadEntity()
            {
                UploadId = model.UploadId,
                ContentType = saveLocalResult.ContentType,
                DropboxFileId = dropSaveResult.DropboxFileId,
                FileSize = saveLocalResult.FileSize,
                OriginalFileName = saveLocalResult.FileName,
                Id = Guid.NewGuid()
            };

            _uploadService.Create(upload);
            if (!_uploadService.SaveChanges())
            {
                _logger.LogError("{date} | Could not save Upload in the database", DateTime.Now);
                return false;
            }

            invoiceFromRepo.Deleted = false;
            invoiceFromRepo.UploadId = upload.Id;

            _invoiceService.Update(invoiceFromRepo);

            bool invoiceUpdateResult = _invoiceService.SaveChanges();

            if (!invoiceUpdateResult)
            {
                _logger.LogError("{date} | Could not update invoice with ID \"{id}\" in the database.", DateTime.Now, invoiceFromRepo.Id);
                return false;
            }

            _logger.LogInformation("{date} | Invoice with ID \"{id}\" successfully updated.", DateTime.Now, invoiceFromRepo.Id);

            return invoiceUpdateResult;
        }

        public bool Update<T>(T model) where T : EventModel
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
