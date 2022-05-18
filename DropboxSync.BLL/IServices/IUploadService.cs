using DropboxSync.BLL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public interface IUploadService : IServiceBase<UploadEntity, Guid>
    {
        public UploadEntity? GetByUploadId(string uploadId);
    }
}
