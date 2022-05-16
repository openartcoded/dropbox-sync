using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.Files;
using DropboxSync.BLL.IServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

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

    internal record OAuthRequest(string oauth1_token, string oauth1_token_secret);
    internal record OAuthResponse(string oauth2_token);

    public class DropboxService : IDropboxService
    {
        private readonly string API_KEY = Environment.GetEnvironmentVariable("DROPBOX_API_KEY") ??
            "";

        private readonly string API_SECRET = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
            "";

        private readonly ILogger _logger;
        private readonly DropboxClient _dropboxClient;

        public string AccessToken { get; private set; } = string.Empty;

        public bool IsOperational
        {
            get => CheckDropboxClient();
        }

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _dropboxClient = new DropboxClient(AccessToken);

            if (!IsOperational)
            {
                _logger.LogError("{date} | The Dropbox Client check failed!", DateTime.Now);
                throw new Exception($"Dropbox Access Token, API Key or API Secret is incorrect!");
            }
        }

        public async Task<string?> SaveUnprocessedFile(string fileName, DateTime createdAt, string absoluteLocalPath,
            FileType fileType, string? fileExtension = null)
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                if (!await GetAccessToken())
                {
                    _logger.LogError("{date} | Access token couldn't be generated!", DateTime.Now);
                    return null;
                }
            }

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

                    var creationResult = await _dropboxClient.Files.UploadAsync(new UploadArg($"{folderDropboxPath}/{fileDropboxName}"),
                        new FileStream(absoluteLocalPath, FileMode.Open));

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

        /// <summary>
        /// Retrieve access token from Dropbox API and set <see cref="AccessToken"/> value.
        /// </summary>
        /// <returns><c>true</c> If a valid access token was retrieved and assigned. <c>false</c> Otherwise</returns>
        private async Task<bool> GetAccessToken()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string keyAndSecret = $"{API_KEY}:{API_SECRET}";
                string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(keyAndSecret));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);

                OAuthRequest oAuthRequest = new OAuthRequest(API_KEY, API_SECRET);
                string serializedRequest = JsonConvert.SerializeObject(oAuthRequest);

                HttpContent content = new StringContent(serializedRequest);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await httpClient.PostAsync("https://api.dropboxapi.com/2/auth/token/from_oauth1", content);
                if (response is null)
                {
                    _logger.LogError("{date} | Api response is null!", DateTime.Now);
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("{date} | The request was unsuccessfull and return this : {message}", DateTime.Now, response.Content);
                    return false;
                }

                OAuthResponse? oAuthResponse = await response.Content.ReadFromJsonAsync<OAuthResponse>();
                if (oAuthResponse is null)
                {
                    _logger.LogError("{date} | Deserialized response is null!", DateTime.Now);
                    return false;
                }

                AccessToken = oAuthResponse.oauth2_token;

                _logger.LogInformation("{date} | Access token has been generated", DateTime.Now);
                return true;
            }
        }

        // TODO : Allow Env variable for root folder
        private async Task<string?> CheckFolderAndCreate(string folderName)
        {
            if (string.IsNullOrEmpty(nameof(folderName))) throw new ArgumentNullException(nameof(folderName));

            //SearchV2Result fileList = await _dropboxClient.Files.SearchV2Async($"/ARTCODED");
            var fileList = await _dropboxClient.Files.ListFolderAsync("/ARTCODED", recursive: true,
                includeHasExplicitSharedMembers: true, includeMountedFolders: true);

            if (fileList is null)
            {
                _logger.LogError("{date} | An error occurred when trying to list folder \"ARTCODED\"", DateTime.Now);
                return null;
            }

            bool firstOccurence = true;

            do
            {
                if (!firstOccurence) fileList = await _dropboxClient.Files.ListFolderContinueAsync(fileList.Cursor);

                foreach (Metadata file in fileList.Entries)
                {
                    if (file.IsFolder && file.PathDisplay.Equals($"/ARTCODED/{folderName}")) return file.AsFolder.PathLower;
                }

                firstOccurence = false;
            }
            while (fileList.HasMore);

            CreateFolderResult value = await _dropboxClient.Files.CreateFolderV2Async($"/ARTCODED/{folderName}");
            return value.Metadata.PathLower;
        }

        private bool CheckDropboxClient()
        {
            string query = "Foo";

            EchoResult? echoResult = Task.Run(async () => await _dropboxClient.Check.UserAsync(query)).Result;

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
