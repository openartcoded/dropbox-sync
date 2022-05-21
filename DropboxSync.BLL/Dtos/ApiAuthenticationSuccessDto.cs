using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Dtos
{
    public class ApiAuthenticationSuccessDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}
