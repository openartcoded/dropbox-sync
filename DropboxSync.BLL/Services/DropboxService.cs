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
        private readonly ILogger _logger;
        private readonly string _accessToken;

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _accessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN", EnvironmentVariableTarget.Machine) ??
                throw new NullReferenceException("The environnement variable [DROPBOX_ACCESS_TOKEN] does not exist " +
                "or does not contain any value!");
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
