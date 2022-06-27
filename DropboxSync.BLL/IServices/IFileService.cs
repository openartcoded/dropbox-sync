using DropboxSync.BLL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public record SavedFile(string RelativePath, string ContentType, string FileName, long FileSize);
    public interface IFileService
    {
        Task<SavedFile?> DownloadFile(string fileId, bool lockFile = false);
        bool Delete(string fileName);
    }
}
