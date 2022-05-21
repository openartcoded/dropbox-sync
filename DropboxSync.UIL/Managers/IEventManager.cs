using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public interface IEventManager
    {
        bool Create<T>(T model) where T : EventModel;
        bool Delete<T>(T model) where T : EventModel;
        bool Update<T>(T model) where T : EventModel;
    }
}
