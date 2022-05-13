using Dropbox.Api;
using Dropbox.Api.Users;
using DropboxSync.BLL.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    // TODO : Test the dropbox StartAsync
    // TODO : Delete all the useless methods
    // TODO : Add CRUD operations
    public class DropboxService : IDropboxService
    {
        private readonly string ACCESS_TOKEN = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN") ??
            "";

        private readonly string API_KEY = Environment.GetEnvironmentVariable("DROPBOX_API_KEY") ??
            "";

        private readonly string API_SECRET = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
            "";

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
        }

        public async Task Authenticate()
        {
            using var dbx = new DropboxClient("sl.BHeYUEgU5HjiJIpMG5INT-EuVyp2djKnmJXj0yDUrBBLZuc8hfae0O7EktePWcuoPvuPSsMvS6osOJJ9ieMi8fZ-qnrkBWkb6Ux_Q4p7eCF7jrnMR5VZS-awDS_4xmbG9NbVb3LTjCRK");
            
            var x = await dbx.Files.ListFolderAsync("");
            var u = await dbx.Users.GetCurrentAccountAsync();
            Console.WriteLine(u.AccountId);
            Console.WriteLine(u.Name);
            Console.WriteLine(u.Email);

            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sl.BHefZqtzmrbzBfIgCAySLCXE22hQFm_dQJtTMMadS6nMjGz098B7LJIzKYYou-z66uzzqajz6HJEvqXQ_CLjdcoiL8UezEyHBd3UuqWvO1Rjo4rNIis7-2zW-RFWH7-k1YN8SSiLSNoh");
            var v = new { query = "Foo" };
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(API_KEY + ":" + API_SECRET);
            string basic = System.Convert.ToBase64String(plainTextBytes);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);

            var response = await _httpClient.PostAsJsonAsync("https://api.dropboxapi.com/2/check/user", v);
            _logger.LogInformation(response.ToString());
        }
    }
}
