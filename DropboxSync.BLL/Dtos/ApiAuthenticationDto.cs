using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Dtos
{
    public class ApiAuthenticationDto
    {
        public string GrantType { get; private set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        public ApiAuthenticationDto()
        {
            GrantType = "client_credentials";
        }
    }
}
