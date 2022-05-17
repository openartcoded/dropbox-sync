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
        Task<DropboxSavedFile?> SaveUnprocessedFile(string fileName, DateTime createdAt, string absoluteLocalPath, FileTypes fileType,
            string? fileExtension = null);

        Task<DropboxMovedFile?> MoveFile(string dropboxFileId, string dropboxFilePath, DateTime fileCreationDate, FileTypes fileType,
            bool isProcess, string? dossierName = null);
        Task<DropboxMovedFile?> UnprocessFile(string dropboxFileId, DateTime fileCreationDate, FileTypes fileType);
        Task<bool> DeleteFile(string fileId);
    }
}
