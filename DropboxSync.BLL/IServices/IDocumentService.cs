using DropboxSync.BLL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    public interface IDocumentService : IServiceBase<DocumentEntity, Guid>
    {
    }
}
