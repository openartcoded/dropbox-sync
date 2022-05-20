using AutoMapper;
using DropboxSync.BLL;
using DropboxSync.BLL.Entities;
using DropboxSync.BLL.IServices;
using DropboxSync.Helpers;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class DocumentManager : IDocumentManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDocumentService _documentService;
        private readonly IUploadService _uploadService;
        private readonly IDropboxService _dropboxService;
        private readonly IFileService _fileService;

        public DocumentManager(ILogger<DocumentManager> logger, IMapper mapper, IDocumentService documentService, IUploadService uploadService,
            IDropboxService dropboxService, IFileService fileService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _documentService = documentService ??
                throw new ArgumentNullException(nameof(documentService));
            _uploadService = uploadService ??
                throw new ArgumentNullException(nameof(uploadService));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
            _fileService = fileService ??
                throw new ArgumentNullException(nameof(fileService));
        }

        public bool Create<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool CreateUpdate(DocumentCreateUpdateModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DocumentEntity? documentFromRepo = _documentService.GetById(Guid.Parse(model.DocumentId));

            if (documentFromRepo is not null)
            {
                UploadEntity? uploadFromRepo = _uploadService.GetDocumentRelatedUpload(Guid.Parse(model.DocumentId));

                if (uploadFromRepo is not null)
                {
                    bool deletionResult = _fileService.Delete(uploadFromRepo.OriginalFileName);

                    if (!deletionResult)
                    {
                        _logger.LogWarning("{date} | Couldn't delete file \"{name}\" from local backup",
                            DateTime.Now, uploadFromRepo.OriginalFileName);
                    }

                    bool dropboxDeletionResult = AsyncHelper.RunSync(() => _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId));

                    if (!dropboxDeletionResult)
                    {
                        _logger.LogWarning("{date} | Couldn't delete file \"{name}\" from dropbox",
                            DateTime.Now, uploadFromRepo.OriginalFileName);
                    }

                    //_uploadService.Delete(uploadFromRepo);
                    //if (!_uploadService.SaveChanges())
                    //{
                    //    _logger.LogError("{date} | Couldn't delete upload with ID \"{id}\" from database", DateTime.Now, uploadFromRepo.Id);
                    //    return false;
                    //}

                    documentFromRepo.Upload = null;
                    documentFromRepo.UploadId = null;

                    _documentService.Update(documentFromRepo);

                    _documentService.SaveChanges();
                }
            }

            SavedFile? localSaveResult = AsyncHelper.RunSync(() => _fileService.DownloadFile(model.UploadId));

            if (localSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't download upload with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

            DropboxSavedFile? dropboxResult = AsyncHelper.RunSync(() =>
                _dropboxService.SaveUnprocessedFileAsync(
                    fileName: localSaveResult.FileName,
                    createdAt: DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                    fileRelativePath: localSaveResult.RelativePath,
                    fileType: FileTypes.Documents));

            if (dropboxResult is null)
            {
                _logger.LogError("{date} | Couldn't save file with ID \"{id}\" in Dropbox", DateTime.Now, model.UploadId);
                return false;
            }

            UploadEntity upload = new UploadEntity(
                uploadId: model.UploadId,
                originalFileName: localSaveResult.FileName,
                dropboxFileId: dropboxResult.DropboxFileId,
                contentType: localSaveResult.ContentType,
                fileSize: localSaveResult.FileSize);

            _uploadService.Create(upload);
            _uploadService.SaveChanges();

            if (documentFromRepo is null)
            {
                documentFromRepo = new DocumentEntity()
                {
                    Upload = upload,
                    CreatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                    UpdatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp),
                    Description = model.Description,
                    Title = model.Title,
                    Id = Guid.Parse(model.DocumentId)
                };

                _documentService.Create(documentFromRepo);
            }
            else
            {
                documentFromRepo.UpdatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp);
                documentFromRepo.UploadId = upload.Id;
                documentFromRepo.Upload = upload;
                documentFromRepo.Description = model.Description;
                documentFromRepo.Title = model.Title;

                _documentService.Update(documentFromRepo);
            }

            if (!_documentService.SaveChanges())
            {
                _logger.LogError("{date} | Document and upload haven't been saved to the database!", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Document with ID \"{docId}\" with upload with ID \"{upId}\" have been successfully saved.",
                DateTime.Now, documentFromRepo.Id, upload.Id);

            return true;
        }

        public bool Delete(DocumentRemoveModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DocumentEntity? documentFromRepo = _documentService.GetById(Guid.Parse(model.DocumentId));

            if (documentFromRepo is null)
            {
                _logger.LogError("{date} | There is no Document in database with ID \"{id}\"", DateTime.Now, model.DocumentId);
                return false;
            }

            UploadEntity? uploadFromRepo = _uploadService.GetDocumentRelatedUpload(Guid.Parse(model.DocumentId));

            if (uploadFromRepo is null)
            {
                _logger.LogError("{date} | There is no upload in database associated to document with ID \"{id}\"",
                    DateTime.Now, documentFromRepo.Id);

                return true;
            }

            bool deletionResult = _fileService.Delete(uploadFromRepo.OriginalFileName);

            if (!deletionResult)
            {
                _logger.LogWarning("{date} | Couldn't delete file \"{name}\" from local backup",
                    DateTime.Now, uploadFromRepo.OriginalFileName);
            }

            _logger.LogInformation("{date} | Successfully deleted local file with name \"{name}\"",
                DateTime.Now, uploadFromRepo.OriginalFileName);

            bool dropboxDeletionResult = AsyncHelper.RunSync(() => _dropboxService.DeleteFile(uploadFromRepo.DropboxFileId));

            if (!dropboxDeletionResult)
            {
                _logger.LogWarning("{date} | Couldn't delete file \"{name}\" from dropbox",
                    DateTime.Now, uploadFromRepo.OriginalFileName);
            }

            _logger.LogInformation("{date} | Successfully deleted file from Dropbox with Dropbox ID \"{id}\"",
                DateTime.Now, uploadFromRepo.DropboxFileId);

            _documentService.Delete(documentFromRepo);

            if (!_documentService.SaveChanges())
            {
                _logger.LogError("{date} | Changes to Document with ID \"{id}\" haven't been saved to the database",
                    DateTime.Now, documentFromRepo.Id);

                return false;
            }

            _logger.LogInformation("{date} | Successfully deleted Document with ID \"{id}\" from database.",
                DateTime.Now, documentFromRepo.Id);

            return true;
        }

        public bool Delete<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }
    }
}
