using AutoMapper;
using DropboxSync.BLL;
using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool Create<T>(T entity) where T : InvoiceGeneratedModel
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            InvoiceEntity? entityToCreate = _mapper.Map<InvoiceEntity>(entity);
            if (entityToCreate is null)
            {
                _logger.LogError("{date} | Couldn't map {entityName} to {entityCreate}",
                    DateTime.Now, nameof(entity), nameof(entityToCreate));
                return false;
            }

            SavedFile? localSaveResult = Task.Run(async () => await _fileService.DownloadFile(entity.UploadId)).Result;
            if (localSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't save file locally", DateTime.Now);
                return false;
            }

            DropboxSavedFile? dropboxSaveResult = Task.Run(async () =>
                await _dropboxService.SaveUnprocessedFile(localSaveResult.FileName, new DateTime(entity.Timestamp),
                    localSaveResult.RelativePath, FileTypes.Invoices, localSaveResult.FileExtension)).Result;

            if (dropboxSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't save file with ID \"{id}\" in Dropbox", DateTime.Now, entity.UploadId);
                return false;
            }

            UploadEntity upload = new UploadEntity()
            {
                UploadId = entity.UploadId,
                ContentType = localSaveResult.ContentType,
                DropboxFileId = dropboxSaveResult.DropboxFileId,
                FileExtention = localSaveResult.FileExtension ?? "UKNOWN",
                FileSize = localSaveResult.FileSize,
                OriginalFileName = localSaveResult.FileName,
                Id = Guid.NewGuid()
            };

            _uploadService.Create(upload);

            if (!_uploadService.SaveChanges())
            {
                _logger.LogError("{date} | The upload couldn't be saved in the database!", DateTime.Now);
                return false;
            }

            _invoiceService.Create(entityToCreate);

            bool saveResult = _invoiceService.SaveChanges();

            if (!saveResult)
            {
                _logger.LogError("{date} | Couldn't save the Invoice in the database", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Invoice and Upload successfully saved in the database", DateTime.Now);

            return saveResult;
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

            bool dropboxDeleteResult = Task.Run(async () => await _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId)).Result;
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

            SavedFile? saveLocalResult = Task.Run(async () => await _fileService.DownloadFile(model.UploadId)).Result;
            if (saveLocalResult is null)
            {
                _logger.LogError("{date} | Could not locally save upload with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

            DropboxSavedFile? dropSaveResult = Task.Run(async () =>
                await _dropboxService.SaveUnprocessedFile(saveLocalResult.FileName, new DateTime(model.Timestamp),
                    saveLocalResult.RelativePath, FileTypes.Invoices, saveLocalResult.FileExtension)).Result;

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
                FileExtention = saveLocalResult.FileExtension ?? "UNKNOWN",
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
