using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IEventManager<T>
        where T : EventModel
    {
        bool Create(T model);
        bool Delete(T model);
        bool Update(T model);
        bool Redirect(string eventJson);
    }
}
