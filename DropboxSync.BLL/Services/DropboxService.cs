using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.Users;
using DropboxSync.BLL.Entities;
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

    // TODO : 1. When Expense is received. Save it to a Unprocessed folder in Dropbox with the next format => [DATE]-[FILENAME].[EXTENSION]
    // TODO : 2. When an Expense is deleted. Delete all its associated files from Dropbox
    // TODO : 1. Create a file in a specific folder => [Year]/[UNPROCESSED]/[INVOICE/EXPENSE]/[FILES] OR [YEAR]/[DOSSIER]/[INVOICES/EXPENSES]/[FILES]

    // File structure in the Dropbox :
    // Dossier closed : [YEAR]/[DATE-DOSSIER_NAME]/[DOSSIER_ZIP]
    // Dossier creation depends on invoices/expenses added to this dossier. On File added to dossier, first it should verify if the dossier exist
    // Files added to dossier : [YEAR]/[DATE-DOSSIER_NAME/[INVOICES/EXPENSES]/[DATE-FILE_NAME.FILE_EXTENSION]
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
        private readonly DropboxClient _dropboxClient;

        public bool IsOperational
        {
            get => CheckDropboxClient();
        }

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
            _dropboxClient = new DropboxClient(ACCESS_TOKEN);

            if (!IsOperational)
            {
                _logger.LogError("{date} | The Dropbox Client check failed!", DateTime.Now);
                throw new Exception($"Dropbox Access Token, API Key or API Secret is incorrect!");
            }
        }

        ~DropboxService()
        {
            _dropboxClient.Dispose();
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

        public async Task<bool> SaveFile(string absoluteFilePath, FileType fileType, bool isProcessed = false, DossierEntity? dossier = null)
        {
            if (string.IsNullOrEmpty(absoluteFilePath)) throw new ArgumentNullException(nameof(absoluteFilePath));

            if (fileType != FileType.Dossier && dossier is null)
            {
                _logger.LogError("{date} | If FileType is not dossier, {dossierEntity} cannot be null!", DateTime.Now, nameof(dossier));
                throw new ArgumentNullException(nameof(dossier));
            }

            if (fileType != FileType.Dossier && isProcessed && dossier is null)
            {
                _logger.LogError("{date} | If file is processed, a dossier must be assigned!", DateTime.Now);
                throw new ArgumentNullException(nameof(dossier));
            }

            if (fileType == FileType.Dossier && dossier is not null)
            {
                _logger.LogWarning("{date} | The entity [{dossierEntity}] cannot be instanciated when file type is equals to \"FileType.Dossier\" ",
                    DateTime.Now, nameof(dossier));
                return false;
            }

            if (isProcessed && fileType == FileType.Dossier)
            {
                _logger.LogWarning("{date} | A dossier cannot be processed!", DateTime.Now);
                return false;
            }

            switch (fileType)
            {
                case FileType.Invoice:

                    if (isProcessed)
                    {
                        DateTime creationDate = DateTime.Now;
                    }
                    else
                    {
                        DateTime creationDate = DateTime.Now;

                    }

                    break;
                case FileType.Expense:
                    break;
                case FileType.Dossier:
                    break;
                default:
                    break;
            }
        }

        private bool CheckDropboxClient()
        {
            string query = "Foo";

            Task<EchoResult> taskResult = Task.Run(async () => await _dropboxClient.Check.UserAsync(query));

            if (taskResult is null) throw new ArgumentNullException(nameof(taskResult));

            if (!taskResult.IsCompletedSuccessfully)
            {
                _logger.LogError("{date} | The Dropbox client check didn't complete successfully!", DateTime.Now);
                return false;
            }

            EchoResult? echoResult = taskResult.Result;
            if (echoResult is null)
            {
                _logger.LogError("{date} | The echo result value is null. The check failed!", DateTime.Now);
                return false;
            }

            if (echoResult.Result.Equals(query)) return true;

            _logger.LogWarning("{date} | Something wrong happened during echo check. More informations are needed", DateTime.Now);
            return false;
        }
    }
}
