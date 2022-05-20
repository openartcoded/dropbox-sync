using AutoMapper;
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

            if (documentFromRepo is null)
            {
                documentFromRepo = new DocumentEntity()
                {
                    Description = model.Description,
                    Title = model.Title,
                    Id = Guid.NewGuid()
                };
            }

            SavedFile? localSaveResult = AsyncHelper.RunSync(() => _fileService.DownloadFile(model.UploadId));

            if (localSaveResult is null)
            {
                _logger.LogError("{date} | Couldn't download upload with ID \"{id}\"", DateTime.Now, model.UploadId);
                return false;
            }

        }

        public bool Delete(DocumentRemoveModel model)
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
    }
}
