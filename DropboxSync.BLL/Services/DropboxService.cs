using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Files;
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

        public async Task<string?> SaveUnprocessedFile(string fileName, DateTime createdAt, string absoluteLocalPath,
            FileType fileType, string? fileExtension = null)
        {
            if (string.IsNullOrEmpty(absoluteLocalPath)) throw new ArgumentNullException(nameof(absoluteLocalPath));

            string requiredFolder = $"{createdAt.Year}/UNPROCESSED/{fileType.ToString().ToUpper()}";

            switch (fileType)
            {
                case FileType.Invoice:

                    string? folderDropboxPath = await CheckFolderAndCreate(requiredFolder);
                    if (string.IsNullOrEmpty(folderDropboxPath))
                    {
                        _logger.LogError("{date} | The folder couldn't be checked nor created!", DateTime.Now);
                        return null;
                    }

                    string fileDropboxName = $"{createdAt.ToString("yyyy.MM.dd")}-{fileName}";

                    if (!string.IsNullOrEmpty(fileExtension)) fileDropboxName = string.Join('.', fileDropboxName, fileExtension);

                    FileRequest creationResult = await _dropboxClient.FileRequests.CreateAsync(fileDropboxName, folderDropboxPath);
                    if (creationResult is null)
                    {
                        _logger.LogError("{date} | File \"{fileName}\" could not be created at path \"{path}\"",
                            DateTime.Now, fileName, folderDropboxPath);
                        return null;
                    }

                    return creationResult.Id;

                case FileType.Expense:
                    break;
                case FileType.Dossier:
                    break;
                default:
                    break;
            }

            return null;
        }

        // TODO : Create ARTCODED FOLDER if doesn't exist
        // TODO : Check if folder has share permissions. Create them otherwise
        private bool Initialize()
        {
            return true;
        }

        private async Task<string?> CheckFolderAndCreate(string folderName)
        {
            if (string.IsNullOrEmpty(nameof(folderName))) throw new ArgumentNullException(nameof(folderName));

            ListFolderResult fileList = await _dropboxClient.Files.ListFolderAsync("ARTCODED");
            if (fileList is null)
            {
                _logger.LogError("{date} | An error occurred when trying to list folder \"ARTCODED\"", DateTime.Now);
                return null;
            }

            while (fileList.HasMore)
            {
                foreach (Metadata file in fileList.Entries)
                {
                    if (file.IsFolder && file.Name.Equals(folderName)) return file.AsFolder.PathLower;
                }

                fileList = await _dropboxClient.Files.ListFolderContinueAsync(fileList.Cursor);
            }

            CreateFolderResult value = await _dropboxClient.Files.CreateFolderV2Async($"ARTCODED/{folderName}");
            return value.Metadata.PathLower;
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

        /// <summary>
        /// Dispose <see cref="DropboxClient"/> private property when <see cref="DropboxService"/> is destroyed 
        /// </summary>
        ~DropboxService()
        {
            _dropboxClient.Dispose();
        }
    }
}
