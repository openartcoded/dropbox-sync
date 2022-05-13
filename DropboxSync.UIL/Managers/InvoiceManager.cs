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

            var entityToRepo = _mapper.Map<InvoiceEntity>(entity);
            _invoiceService.Create(entityToRepo);

            string? result = Task.Run(async () => await _fileService.DownloadFile(entity.UploadId)).Result;
            if (string.IsNullOrEmpty(result))
            {
                _logger.LogError("{date} | File couldn't be saved locally", DateTime.Now);
                return false;
            }

            FileInfo? infos = new FileInfo(result);
            if (infos is null)
            {
                _logger.LogError("{date} | Could not read file informations", DateTime.Now);
                return false;
            }

            UploadEntity upload = new UploadEntity()
            {
                ContentType = infos.Extension.Remove(0, 1),
                FileExtention = infos.Extension,
                FileSize = infos.Length,
                OriginalFileName = infos.Name,
            };

            string? fileDropboxId = Task.Run(async () =>
                await _dropboxService.SaveUnprocessedFile(upload.OriginalFileName, DateTime.Now, Path.GetFullPath(result), FileType.Invoice)).Result;

            if (fileDropboxId is null)
            {
                _logger.LogWarning("{date} | The file couldn't be saved in dropbox but is saved locally at path \"{filePath}\"",
                    DateTime.Now, Path.GetFullPath(result));
                return false;
            }

            upload.DropboxFileId = fileDropboxId;

            entityToRepo.Upload = upload;
            if (!_invoiceService.SaveChanges()) return false;

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

        bool IEventManager.Create<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
