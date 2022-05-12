using Dropbox.Api;
using Dropbox.Api.Users;
using DropboxSync.BLL.IServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    // TODO : Test the dropbox StartAsync
    // TODO : Delete all the useless methods
    // TODO : Add CRUD operations
    public class DropboxService : IDropboxService
    {
        private const string LOOPBACK_HOST = "http://localhost:52475/";

        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _accessToken;

        public string AccessToken { get; private set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string AuthenticationUrl { get; set; } = string.Empty;

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _apiKey = Environment.GetEnvironmentVariable("DROPBOX_API_KEY")
                ?? throw new NullReferenceException("No api key for Dropbox has been registered in the environnement");
            _apiSecret = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
                throw new NullReferenceException("No api secret for dropbox has been registered in the environnement");
            //_accessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN")
            //    ?? throw new NullReferenceException(nameof(_accessToken));

            //AccessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN")
            //    ?? Task.Run(() => GetDropboxAccessToken()).Result
            //    ?? throw new NullReferenceException("No access token was obtainable!");
            //RefreshToken = Environment.GetEnvironmentVariable("DROPBOX_REFRESH_TOKEN")
            //    ?? Task.Run(() => GetDropboxRefreshToken()).Result
            //    ?? throw new NullReferenceException("No refresh token was obtainable!");
        }

        public async Task StartAsync()
        {
            using (var dropboxClient = new DropboxClient(_accessToken))
            {
                FullAccount? full = await dropboxClient.Users.GetCurrentAccountAsync();
                if (full is null) throw new NullReferenceException(nameof(full));

                _logger.LogInformation("Connected to Dropbox with [Account ID \"{accountId}\" | Email : \"{email}\" | " +
                    "Display name : \"{name}\"]", full.AccountId, full.Email, full.Name.DisplayName);
            }
        }
    }
}
