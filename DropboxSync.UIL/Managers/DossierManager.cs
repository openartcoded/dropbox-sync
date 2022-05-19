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
    public class DossierManager : IDossierManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDossierService _dossierService;
        private readonly IFileService _fileService;
        private readonly IDropboxService _dropboxService;
        private readonly IUploadService _uploadService;
        private readonly IExpenseService _expenseService;
        private readonly IInvoiceService _invoiceService;

        public DossierManager(ILogger<DossierManager> logger, IMapper mapper, IDossierService dossierService, IFileService fileService,
            IDropboxService dropboxService, IUploadService uploadService, IExpenseService expenseService, IInvoiceService invoiceService)
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
            _expenseService = expenseService ??
                throw new ArgumentNullException(nameof(expenseService));
            _invoiceService = invoiceService ??
                throw new ArgumentNullException(nameof(invoiceService));
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

            bool dropboxDossierRemovalResult = Task.Run(async () =>
                await _dropboxService.DeleteDossierAsync(dossierFromRepo.Name, dossierFromRepo.CreatedAt)).Result;

            if (!dropboxDossierRemovalResult)
            {
                _logger.LogError("{date} | An error occured while trying to delete dossier \"{name}\". Please read precedent logs " +
                    "to understand.", DateTime.Now, dossierFromRepo.Name);

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
            if (model is null) throw new ArgumentNullException(nameof(model));

            DossierEntity? dossierFromRepo = _dossierService.GetById(Guid.Parse(model.DossierId));

            if (dossierFromRepo is null)
            {
                _logger.LogError("{date} | There is no dossier with ID \"{id}\"", DateTime.Now, model.DossierId);
                return false;
            }

            foreach (string? expenseId in model.ExpenseIds)
            {
                if (string.IsNullOrEmpty(expenseId))
                {
                    _logger.LogWarning("{date} | An expense ID in the list is null or an empty string.", DateTime.Now);
                    continue;
                }

                ExpenseEntity? expenseFromRepo = _expenseService.GetById(Guid.Parse(expenseId));
                if (expenseFromRepo is null)
                {
                    _logger.LogWarning("{date} | There is no expense in the database with ID \"{id}\"", DateTime.Now, expenseId);
                    continue;
                }

                IEnumerable<UploadEntity>? uploadsFromRepo = _uploadService.GetExpenseRelatedUploads(expenseFromRepo.Id);
                if (uploadsFromRepo is null)
                {
                    _logger.LogWarning("{date} | There is no uploads for expense with ID : \"{id}\"", DateTime.Now, expenseId);
                    continue;
                }

                foreach (UploadEntity? upload in uploadsFromRepo)
                {
                    if (upload is null)
                    {
                        _logger.LogWarning("{date} | An upload of type \"{uploadType}\" in \"{listName}\" is null.",
                            DateTime.Now, typeof(UploadEntity), nameof(uploadsFromRepo));
                        continue;
                    }

                    DropboxMovedFile? dropboxMovedFile = Task.Run(async () =>
                        await _dropboxService
                            .MoveFile(upload.DropboxFileId, expenseFromRepo.CreatedAt, FileTypes.Expenses, true, dossierFromRepo.Name))
                            .Result;

                    if (dropboxMovedFile is null)
                    {
                        _logger.LogError("{date} | Couldn't move file \"{id}\"", DateTime.Now, upload.Id);
                        continue;
                    }

                    _logger.LogInformation("{date} | Upload with ID \"{id}\" moved from \"{oldPath}\" to \"{newPath}\"",
                        DateTime.Now, upload.UploadId, dropboxMovedFile.OldPath, dropboxMovedFile.NewPath);
                }
            }

            return true;
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
