using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.Files;
using DropboxSync.BLL.IServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
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

    internal record AccessTokenResponse(string access_token, string token_type, int expires_in, string refresh_token, string scope,
        string uid, string account_id);

    internal record RefreshAccessTokenResponse(string access_token, string token_type, int expires_in);

    public class DropboxService : IDropboxService
    {
        private readonly string API_KEY = Environment.GetEnvironmentVariable("DROPBOX_API_KEY") ??
            "";

        private readonly string API_SECRET = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
            "";

        /// <summary>
        /// Retrieves the declared root folder in Dropbox. If the environnement variable is null then the next folder is used / created:
        /// <c>OPENARTCODED</c>
        /// </summary>
        private readonly string ROOT_FOLDER = Environment.GetEnvironmentVariable("DROPBOX_ROOT_FOLDER") ??
            "OPENARTCODED";

        private readonly ILogger _logger;
        private readonly DropboxClient _dropboxClient;

        public string AccessToken { get; private set; } = string.Empty;
        public string RefreshToken { get; private set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;

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

        /// <summary>
        /// Ask to the Dropbox API a new access token, refresh token and everything else. To make it work, user should manually navigate to the
        /// given url in the <c>Readme</c> file and retrieve the <c>code</c> received by Dropbox.
        /// </summary>
        /// <returns></returns>
        private async Task GetAccessToken()
        {
            using HttpClient httpClient = new HttpClient();
            string authorizationScheme = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{API_KEY}:{API_SECRET}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationScheme);

            KeyValuePair<string, string>[] data = new[]
            {
                new KeyValuePair<string, string>("code",""),
                new KeyValuePair<string, string>("grant_type","authorization_code")
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(data);
            HttpResponseMessage response = await httpClient.PostAsync("https://api.dropboxapi.com/oauth2/token", content);
            if (response is null)
            {
                _logger.LogError("{date} | {responseType} returned null", DateTime.Now, typeof(HttpResponseMessage));
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{date} | HTTP Request failed with status : \"{status}\".", DateTime.Now, response.StatusCode);
                return;
            }

            if (response.Content is null)
            {
                _logger.LogError("{date} | HTTP Request content is null", DateTime.Now);
                return;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("{date} | HTTP Request response received : {response}", DateTime.Now, jsonResponse);

            AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(jsonResponse);
            if (accessTokenResponse is null)
            {
                _logger.LogError("{date} | Deserialized object of type \"{type}\" is null", DateTime.Now, typeof(AccessTokenResponse));
                return;
            }

            if (string.IsNullOrEmpty(accessTokenResponse.access_token))
            {
                _logger.LogError("{date} | The access token field in {atResponse} is null or empty!",
                    DateTime.Now, nameof(accessTokenResponse));
                return;
            }

            if (string.IsNullOrEmpty(accessTokenResponse.refresh_token))
            {
                _logger.LogError("{date} | The refresh token field in {atResponse} is null or empty!",
                    DateTime.Now, nameof(accessTokenResponse));
                return;
            }

            Environment.SetEnvironmentVariable("DROPBOX_REFRESH_TOKEN", accessTokenResponse.refresh_token);
            AccessToken = accessTokenResponse.access_token;
        }

        /// <summary>
        /// Retrieves a new access token from Dropbox API. If the <see cref="RefreshToken"/> is <c>null</c> or <c>empty</c> or environnement variable 
        /// containing the refresh token at <c>DROPBOX_REFRESH_TOKEN</c> is also <c>null</c> or <c>empty</c> then logger displays the error in the 
        /// console and break the method.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshAccessToken()
        {
            if (string.IsNullOrEmpty(RefreshToken))
            {
                RefreshToken = Environment.GetEnvironmentVariable("DROPBOX_REFRESH_TOKEN") ?? "";
                if (string.IsNullOrEmpty(RefreshToken))
                {
                    _logger.LogError("{date} | Refresh token couldn't be retrieved from environnement variable. Please follow the tutorial " +
                        "for proper configuration!", DateTime.Now);
                    return;
                }
            }

            using HttpClient httpClient = new HttpClient();

            string authorizationScheme = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{API_KEY}:{API_SECRET}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationScheme);

            KeyValuePair<string, string>[] data = new[]
            {
                new KeyValuePair<string, string>("grant_type","refresh_token"),
                new KeyValuePair<string, string>("refresh_token", RefreshToken)
            };

            FormUrlEncodedContent content = new FormUrlEncodedContent(data);
            HttpResponseMessage response = await httpClient.PostAsync("https://api.dropboxapi.com/oauth2/token", content);

            if (response is null)
            {
                _logger.LogError("{date} | HTTP Request's response is null", DateTime.Now);
                return;
            }

            if (response.Content is null)
            {
                _logger.LogError("{date} | HTTP Request's content is null", DateTime.Now);
                return;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(jsonResponse))
            {
                _logger.LogError("{date} | HTTP Request's content is an empty string!", DateTime.Now);
                return;
            }

            RefreshAccessTokenResponse refreshAccessTokenResponse = JsonConvert.DeserializeObject<RefreshAccessTokenResponse>(jsonResponse);
            if (refreshAccessTokenResponse is null)
            {
                _logger.LogError("{date} | HTTP Request's content as json couldn't be deserialized to type {type}",
                    DateTime.Now, typeof(RefreshAccessTokenResponse));
                return;
            }

            AccessToken = refreshAccessTokenResponse.access_token;
            TokenType = refreshAccessTokenResponse.token_type;
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
