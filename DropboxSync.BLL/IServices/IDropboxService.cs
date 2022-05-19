using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public record DropboxSavedFile(string DropboxFileId, string DropboxFilePath);
    public record DropboxMovedFile(string OldPath, string NewPath);

    public interface IDropboxService
    {
        Task<DropboxSavedFile?> SaveUnprocessedFile(string fileName, DateTime createdAt, string fileRelativePath, FileTypes fileType,
            string? fileExtension = null);
        Task<bool> CreateDossierAsync(string dossierName, DateTime createdAt, FileTypes fileType);
        Task<bool> DeleteDossierAsync(string dossierName, DateTime createdAt);
        Task<DropboxSavedFile?> SaveDossierAsync(string dossierName, string fileName, string dossierRelativePath, DateTime createdAt);
        Task<DropboxMovedFile?> MoveFileAsync(string dropboxFileId, DateTime fileCreationDate, FileTypes movingFilesType, bool isProcess,
            string? dossierName = null);
        Task<DropboxMovedFile?> UnprocessFile(string dropboxFileId, DateTime fileCreationDate, FileTypes fileType);
        Task<bool> DeleteFile(string dropboxId);
    }
}
