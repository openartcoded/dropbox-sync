using AutoMapper;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Managers
{
    public class DocumentManager : IDocumentManager
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public bool Create<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool CreateUpdate(DocumentCreateUpdateModel model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(DocumentRemoveModel model)
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(T model) where T : EventModel
        {
            throw new NotImplementedException();
        }
    }
}
