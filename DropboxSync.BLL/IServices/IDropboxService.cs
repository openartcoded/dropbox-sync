using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public interface IDropboxService
    {
        Task<string?> SaveUnprocessedFile(string fileName, DateTime createdAt, string absoluteLocalPath,
            FileType fileType, string? fileExtension = null);
    }
}
