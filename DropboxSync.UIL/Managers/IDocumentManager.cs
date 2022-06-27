using DropboxSync.UIL.Attributes;
using DropboxSync.UIL.Enums;
using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IDocumentManager : IEventManager
    {
        [MethodEvent(typeof(DocumentCreateUpdateModel), nameof(BrokerEvent.AdministrativeDocumentAddedOrUpdated))]
        bool CreateUpdate(DocumentCreateUpdateModel model);

        [MethodEvent(typeof(DocumentRemoveModel), nameof(BrokerEvent.AdministrativeDocumentRemoved))]
        bool Delete(DocumentRemoveModel model);
    }
}
