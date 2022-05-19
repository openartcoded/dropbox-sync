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
    public class DossierManager : IDossierManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDossierService _dossierService;
        private readonly IFileService _fileService;
        private readonly IDropboxService _dropboxService;
        private readonly IUploadService _uploadService;

        public DossierManager(ILogger<DossierManager> logger, IMapper mapper, IDossierService dossierService, IFileService fileService,
            IDropboxService dropboxService, IUploadService uploadService)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _dossierService = dossierService ??
                throw new ArgumentNullException(nameof(dossierService));
            _fileService = fileService ??
                throw new ArgumentNullException(nameof(fileService));
            _dropboxService = dropboxService ??
                throw new ArgumentNullException(nameof(dropboxService));
            _uploadService = uploadService ??
                throw new ArgumentNullException(nameof(uploadService));
        }

        public bool CloseDossier(DossierCloseModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity? dossierFromRepo = _dossierService.GetById(Guid.Parse(model.DossierId));
            if (dossierFromRepo is null)
            {
                _logger.LogError("{date} | There is not Dossier in the database with ID : \"{dossierId}\"", DateTime.Now, model.DossierId);
                return false;
            }

            SavedFile? localSaveResult = Task.Run(async () => await _fileService.DownloadFile(model.UploadId)).Result;
            if (localSaveResult is null)
            {
                _logger.LogError("{date} | An error occured while trying to localy save file with ID \"{fileId}\"",
                    DateTime.Now, model.UploadId);
                return false;
            }

            DropboxSavedFile? dropboxSavedFile = _dropboxService.SaveDossierAsync(dossierFromRepo.Name, localSaveResult.FileName,
                localSaveResult.RelativePath, dossierFromRepo.CreatedAt).Result;

            if (dropboxSavedFile is null)
            {
                _logger.LogError("{date} | Could not save dossier with ID \"{id}\" in Dropbox", DateTime.Now, dossierFromRepo.Id);
                return false;
            }

            UploadEntity upload = new UploadEntity()
            {
                ContentType = localSaveResult.ContentType,
                DropboxFileId = dropboxSavedFile.DropboxFileId,
                FileExtention = localSaveResult.FileExtension ?? "UNKNOWN",
                FileSize = localSaveResult.FileSize,
                OriginalFileName = localSaveResult.FileName,
                UploadId = model.UploadId,
                Id = Guid.NewGuid()
            };

            _uploadService.Create(upload);

            if (!_uploadService.SaveChanges())
            {
                _logger.LogError("{date} | Could not save file with ID \"{uploadId}\" to the database.", DateTime.Now, model.UploadId);
                return false;
            }

            dossierFromRepo.UpdatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp);
            dossierFromRepo.IsClosed = true;

            _dossierService.Update(dossierFromRepo);

            if (!_dossierService.SaveChanges())
            {
                _logger.LogError("{date} | Could not save dossier with ID \"{dossierId}\" in the database.", DateTime.Now, dossierFromRepo.Id);
                return false;
            }

            _logger.LogInformation("{date} | Dossier successfully created localy, in Dropbox and in the database", DateTime.Now);

            return true;
        }

        public bool Create<T>(T model) where T : DossierCreateModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity dossierToCreate = _mapper.Map<DossierEntity>(model);

            bool dropboxResult = Task.Run(async () =>
                await _dropboxService.CreateDossierAsync(dossierToCreate.Name, dossierToCreate.CreatedAt, BLL.FileTypes.Dossiers)).Result;

            if (!dropboxResult)
            {
                _logger.LogError("{date} | Couldn't create dossier in Dropbox", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Dossier \"{name}\" created in Dropbox", DateTime.Now, dossierToCreate.Name);

            _dossierService.Create(dossierToCreate);

            if (!_dossierService.SaveChanges())
            {
                _logger.LogError("{date} | Couldn't create dossier in the database", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Dossier \"{name}\" created in the database", DateTime.Now, dossierToCreate.Name);

            return true;
        }

        public bool Delete<T>(T model) where T : DossierDeleteModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity? dossierFromRepo = _dossierService.GetById(Guid.Parse(model.DossierId));
            if (dossierFromRepo is null)
            {
                _logger.LogError("{date} | There is no dossier in the database with ID : {id}", DateTime.Now, model.DossierId);
                return false;
            }

            _dossierService.Delete(dossierFromRepo);

            if (!_dossierService.SaveChanges())
            {
                _logger.LogError("{date} | Couldn't delete dossier \"{id}\" with name {name} from the database!",
                    DateTime.Now, dossierFromRepo.Id, dossierFromRepo.Name);
                return false;
            }

            _logger.LogInformation("{date} | Dossier \"{name}\" with ID \"{id}\" successfully deleted from the database.",
                DateTime.Now, dossierFromRepo.Name, dossierFromRepo.Id);

            return true;
        }

        public bool Recall(DossierRecallForModificationModel model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity? dossierFromRepo = _dossierService.GetById(Guid.Parse(model.DossierId));
            if (dossierFromRepo is null)
            {
                _logger.LogError("{date} | There is no dossier with ID \"{id}\" in the database.", DateTime.Now, model.DossierId);
                return false;
            }

            dossierFromRepo.DueVat = model.TvaDue;
            dossierFromRepo.UpdatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp);

            _dossierService.Update(dossierFromRepo);

            if (!_dossierService.SaveChanges())
            {
                _logger.LogError("{date} | Couldn't update dossier with ID \"{id}\"", DateTime.Now, dossierFromRepo.Id);
                return false;
            }

            _logger.LogInformation("{date} | Successfully updated dossier \"{id}\"", DateTime.Now, dossierFromRepo.Id);

            return true;
        }

        public bool Update<T>(T model) where T : DossierUpdateModel
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity? dossierFromRepo = _dossierService.GetById(Guid.Parse(model.DossierId));
            if (dossierFromRepo is null)
            {
                _logger.LogError("{date} | There is no dossier with ID \"{id}\" in the database", DateTime.Now, model.DossierId);
                return false;
            }

            dossierFromRepo.Name = model.Name;
            dossierFromRepo.Description = model.Description;
            dossierFromRepo.DueVat = model.TvaDue;
            dossierFromRepo.UpdatedAt = DateTimeHelper.FromUnixTimestamp(model.Timestamp);

            _dossierService.Update(dossierFromRepo);

            if (!_dossierService.SaveChanges())
            {
                _logger.LogError("{date} | Couldn't update dossier with ID \"{id}\"", DateTime.Now, dossierFromRepo.Id);
                return false;
            }

            _logger.LogInformation("{date} | Successfully updated dossier \"{name}\" with ID \"{id}\"",
                DateTime.Now, dossierFromRepo.Name, dossierFromRepo.Id);

            return true;
        }

        public bool AddExpense(DossierExpensesAddedModel model)
        {
            throw new NotImplementedException();
        }

        public bool AddInvoice(DossierInvoiceAddedModel model)
        {
            throw new NotImplementedException();
        }

        public bool RemoveExpense(DossierExpenseRemovedModel model)
        {
            throw new NotImplementedException();
        }

        public bool RemoveInvoice(DossierInvoiceRemovedModel model)
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

        bool IEventManager.Update<T>(T model)
        {
            throw new NotImplementedException();
        }
    }
}
