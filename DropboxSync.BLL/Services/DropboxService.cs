using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.FileRequests;
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

        /// <summary>
        /// Create an instance of <see cref="DropboxService"/>.
        /// </summary>
        /// <param name="logger">Service's <see cref="ILogger"/> implementation</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            string? refreshToken = Environment.GetEnvironmentVariable("DROPBOX_REFRESH_TOKEN");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogInformation("{date} | Refresh token couldn't be retrieved from environnement variable.", DateTime.Now);

                Console.WriteLine("Before continuing please follow copy and paste the next url in your web browser. After you accepted everything, " +
                "please enter the code you received");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("IMPORTANT: DO NOT CHANGE ANYTHING IN THE URL! COPY AND PASTE ONLY THE CODE YOU RECEIVED!");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"https://www.dropbox.com/oauth2/authorize?client_id={API_KEY}&response_type=code&token_access_type=offline");
                string? enteredCode = Console.ReadLine();

                while (string.IsNullOrEmpty(enteredCode))
                {
                    Console.WriteLine("Please enter the code!");
                    enteredCode = Console.ReadLine();
                }

                Task.Run(async () => await GetAccessToken(enteredCode)).Wait(10000);

                if (string.IsNullOrEmpty(AccessToken)) throw new Exception($"Operation failed. Please read precedent logs to understand " +
                    $"the error");
            }

            _dropboxClient = new DropboxClient(AccessToken);

            if (!CheckDropboxClient()) throw new Exception($"An error occured during Dropbox Client checkout. Please read the precedent " +
                $"logs to understand the error");

            if (!Task.Run(async () => await VerifyRootFolder()).Result)
                throw new Exception($"An error occured during folder checkout or creation. Please read the precedent logs to understand " +
                    $"the error");
        }

        public async Task<DropboxSavedFile?> SaveUnprocessedFile(string fileName, DateTime createdAt, string fileRelativePath,
            FileTypes fileType, string? fileExtension = null)
        {
            if (string.IsNullOrEmpty(fileRelativePath)) throw new ArgumentNullException(nameof(fileRelativePath));

            DropboxSavedFile? finalOuput = null;

            string requiredFolder = $"{createdAt.Year}/UNPROCESSED/{fileType.ToString().ToUpper()}";
            string? folderDropboxPath = await CheckFolderAndCreate(requiredFolder);
            if (string.IsNullOrEmpty(folderDropboxPath))
            {
                _logger.LogError("{date} | The folder couldn't be checked nor created!", DateTime.Now);
                return finalOuput;
            }

            string fileDropboxName = $"{createdAt.ToString("yyyy.MM.dd HHmm")}-{fileName}";

            FileMetadata creationResult = await _dropboxClient.Files.UploadAsync(new UploadArg($"{folderDropboxPath}/{fileDropboxName}"),
                        new FileStream(fileRelativePath, FileMode.Open));

            if (creationResult is null)
            {
                _logger.LogError("{date} | File \"{fileName}\" could not be created at path \"{path}\"",
                    DateTime.Now, fileName, folderDropboxPath);
                return finalOuput;
            }

            string dropboxId = creationResult.Id.Substring(creationResult.Id.IndexOf(':') + 1);

            finalOuput = new DropboxSavedFile(dropboxId, creationResult.PathDisplay);

            return finalOuput;
        }

        public async Task<DropboxMovedFile?> MoveFile(string dropboxFileId, string dropboxFilePath, DateTime fileCreationDate,
            FileTypes fileType, bool isProcess, string? dossierName = null)
        {
            if (string.IsNullOrEmpty(dropboxFileId)) throw new ArgumentNullException(nameof(dropboxFileId));
            if (string.IsNullOrEmpty(dropboxFilePath)) throw new ArgumentNullException(nameof(dropboxFilePath));
            if (string.IsNullOrEmpty(dossierName) && isProcess) throw new ArgumentNullException(nameof(dossierName));

            DropboxMovedFile? finalOutput = null;

            // TODO : Check if dropboxFilePath is the correct path of the file.
            SearchV2Result searchResult = await _dropboxClient.Files.SearchV2Async(dropboxFilePath);

            if (searchResult is null)
            {
                _logger.LogError("{date} | There is no file with id \"{fileId}\" registered in Dropbox", DateTime.Now, dropboxFilePath);
                return finalOutput;
            }

            if (searchResult.Matches.Count != 1)
            {
                _logger.LogError("{date} | More than one file was found at the location \"{filePath}\"", DateTime.Now, dropboxFilePath);
                return finalOutput;
            }

            string matchPath = searchResult.Matches.First().Metadata.AsMetadata.Value.PathDisplay;
            if (!matchPath.Equals(dropboxFilePath))
            {
                _logger.LogError("{date} | The retrieved match's path from dropbox is different of the provided file path. " +
                    "Dropbox math file path\n\"{dropboxPath}\"\nGiven path:\n\"{filePath}\"", DateTime.Now, matchPath, dropboxFilePath);
                return finalOutput;
            }

            RelocationResult relocationResult;
            string newPath;


            if (isProcess)
            {
                newPath = $"/{ROOT_FOLDER}/{fileCreationDate.Year}/{FileTypes.Dossiers.ToString().ToUpper()}/{dossierName}/{fileType.ToString().ToUpper()}/";
            }
            else
            {
                newPath = $"/{ROOT_FOLDER}/{fileCreationDate.Year}/{fileType.ToString().ToUpper()}/";
            }

            string? verifiedPath = await VerifyFolderExist(newPath, true);

            if (string.IsNullOrEmpty(verifiedPath))
            {
                _logger.LogError("{date} | No folder exist at path \"{path}\" and it couldn't be created!", DateTime.Now, newPath);
                return null;
            }

            relocationResult = await _dropboxClient.Files.MoveV2Async(dropboxFilePath, verifiedPath,
                allowSharedFolder: true, allowOwnershipTransfer: true);

            if (relocationResult is null)
            {
                _logger.LogError("{date} | The relocation failed, returning a \"null\"", DateTime.Now);
                return null;
            }

            finalOutput = new DropboxMovedFile(dropboxFilePath, verifiedPath);

            return finalOutput;
        }

        public Task<DropboxMovedFile?> UnprocessFile(string dropboxFileId, DateTime fileCreationDate, FileTypes fileType)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteFile(string dropboxId)
        {
            if (string.IsNullOrEmpty(dropboxId)) throw new ArgumentNullException(nameof(dropboxId));

            Metadata metadata = await _dropboxClient.Files.GetMetadataAsync($"id:{dropboxId}");
            if (metadata is null)
            {
                _logger.LogError("{date} | No file with ID \"{fileId}\" could have been found.", DateTime.Now, dropboxId);
                return false;
            }

            DeleteResult deleteResult = await _dropboxClient.Files.DeleteV2Async(metadata.PathDisplay);
            if (deleteResult is null)
            {
                _logger.LogError("{date} | The removal of the file failed. {obj} is null", DateTime.Now, nameof(deleteResult));
                return false;
            }

            _logger.LogInformation("{date} | File with ID \"{id}\" have been successfully removed!", DateTime.Now, dropboxId);

            return true;
        }

        /// <summary>
        /// Ask to the Dropbox API a new access token, refresh token and everything else. To make it work, user should manually navigate to the
        /// given url in the <c>Readme</c> file and retrieve the <c>code</c> received by Dropbox.
        /// </summary>
        /// <returns></returns>
        private async Task GetAccessToken(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

            using HttpClient httpClient = new HttpClient();
            string authorizationScheme = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{API_KEY}:{API_SECRET}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationScheme);

            KeyValuePair<string, string>[] data = new[]
            {
                new KeyValuePair<string, string>("code",code),
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

        /// <summary>
        /// First verify Dropbox if any folder with value of <see cref="ROOT_FOLDER"/> exists. If it doesn't exist, then create the folder.
        /// </summary>
        /// <returns><c>true</c> If the folder exists or if the creation went well. <c>false</c> Otherwise</returns>
        private async Task<bool> VerifyRootFolder()
        {
            ListFolderResult listFolderResult = await _dropboxClient.Files.ListFolderAsync("", recursive: true,
                includeHasExplicitSharedMembers: true, includeMountedFolders: true);

            if (listFolderResult is null)
            {
                _logger.LogError("{date} | Impossible to list folders in Dropbox.", DateTime.Now);
                return false;
            }

            bool firstOccurence = true;

            do
            {
                if (!firstOccurence) listFolderResult = await _dropboxClient.Files.ListFolderContinueAsync(listFolderResult.Cursor);

                foreach (Metadata file in listFolderResult.Entries)
                {
                    if (file.IsFolder && file.PathDisplay.Equals($"/{ROOT_FOLDER}"))
                    {
                        _logger.LogInformation("{date} | Folder {name} exist.", DateTime.Now, ROOT_FOLDER);
                        return true;
                    }
                }
            }
            while (listFolderResult.HasMore);

            CreateFolderResult createFolderResult = await _dropboxClient.Files.CreateFolderV2Async($"/{ROOT_FOLDER}");

            if (createFolderResult is null)
            {
                _logger.LogError("{date} | The folder creation result is null", DateTime.Now);
                return false;
            }

            bool isFolder = createFolderResult.Metadata.IsFolder;

            if (!isFolder)
            {
                _logger.LogError("{date} | Folder was created but it is not a file in Dropbox.", DateTime.Now);
                return false;
            }

            _logger.LogInformation("{date} | Folder has been created at root level in Dropbox", DateTime.Now);
            return isFolder;
        }

        // TODO : Allow Env variable for root folder
        private async Task<string?> CheckFolderAndCreate(string folderName)
        {
            if (string.IsNullOrEmpty(nameof(folderName))) throw new ArgumentNullException(nameof(folderName));

            //SearchV2Result fileList = await _dropboxClient.Files.SearchV2Async($"/ARTCODED");
            ListFolderResult fileList = await _dropboxClient.Files.ListFolderAsync($"/{ROOT_FOLDER}", recursive: true,
                includeHasExplicitSharedMembers: true, includeMountedFolders: true);

            if (fileList is null)
            {
                _logger.LogError("{date} | An error occurred when trying to list folder \"{rootF}\"", DateTime.Now, ROOT_FOLDER);
                return null;
            }

            bool firstOccurence = true;

            do
            {
                if (!firstOccurence) fileList = await _dropboxClient.Files.ListFolderContinueAsync(fileList.Cursor);

                foreach (Metadata file in fileList.Entries)
                {
                    if (file.IsFolder && file.PathDisplay.Equals($"/{ROOT_FOLDER}/{folderName}")) return file.AsFolder.PathLower;
                }

                firstOccurence = false;
            }
            while (fileList.HasMore);

            CreateFolderResult value = await _dropboxClient.Files.CreateFolderV2Async($"/{ROOT_FOLDER}/{folderName}");
            return value.Metadata.PathLower;
        }

        /// <summary>
        /// Verify if the folder at full dropbox path <paramref name="folderFullPath"/> exist in Dropbox. If <paramref name="createIfDontExist"/>
        /// is <c>true</c> then the folder is created.
        /// </summary>
        /// <param name="folderFullPath">Dropbox's folder full path</param>
        /// <param name="createIfDontExist">If <c>true</c>, creates the folder at the researched destination</param>
        /// <returns>
        /// <c>null</c> if listing at path <see cref="ROOT_FOLDER"/> failed or if folder isn't found and <paramref name="createIfDontExist"/> is false
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<string?> VerifyFolderExist(string folderFullPath, bool createIfDontExist = false)
        {
            if (string.IsNullOrEmpty(folderFullPath)) throw new ArgumentNullException(nameof(folderFullPath));

            ListFolderResult listFolderResult = await _dropboxClient.Files.ListFolderAsync($"/{ROOT_FOLDER}");
            if (listFolderResult is null)
            {
                _logger.LogError("{date} | List folder returned a null object for path {rootPath}", DateTime.Now, $"/{ROOT_FOLDER}");
                return null;
            }

            bool firstOccurence = true;

            do
            {
                if (!firstOccurence) listFolderResult = await _dropboxClient.Files.ListFolderContinueAsync(listFolderResult.Cursor);

                foreach (Metadata metadata in listFolderResult.Entries)
                {
                    if (metadata.IsFolder && metadata.PathDisplay.Equals(folderFullPath)) return metadata.AsFolder.PathDisplay;
                }

                firstOccurence = false;
            }
            while (listFolderResult.HasMore);

            if (!createIfDontExist) return null;

            return await CreateFolder(folderFullPath);
        }

        /// <summary>
        /// Create a folder at the destination path. The destination path must be absolute, meaning it starts with "/" followed by the
        /// name defined in <see cref="ROOT_FOLDER"/> and the rest defined by the user.
        /// </summary>
        /// <param name="folderFullPath">Non nullable or empty full dropbox path where we create the folder</param>
        /// <returns>
        /// <c>null</c> If the folder couldn't be created or if it created something else than a folder. The Dropbox display full path
        /// otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<string?> CreateFolder(string folderFullPath)
        {
            if (string.IsNullOrEmpty(folderFullPath)) throw new ArgumentNullException(nameof(folderFullPath));

            CreateFolderResult? folderMetadata = await _dropboxClient.Files.CreateFolderV2Async(folderFullPath);
            if (folderMetadata is null)
            {
                _logger.LogError("{date} | Folder at path \"{path}\" couldn't be created!", DateTime.Now, folderFullPath);
                return null;
            }

            if (!folderMetadata.Metadata.IsFolder)
            {
                _logger.LogError("{date} | Something was created at the destination \"{path}\" but not a folder.",
                    DateTime.Now, folderFullPath);
                return null;
            }

            return folderMetadata.Metadata.AsFolder.PathDisplay;
        }

        // TODO : If access token expired exception happens. Refresh the token
        private bool CheckDropboxClient()
        {
            string query = "Foo";

            try
            {
                EchoResult? echoResult = Task.Run(async () => await _dropboxClient.Check.UserAsync(query)).Result;

                if (echoResult is null)
                {
                    _logger.LogError("{date} | The echo result value is null. The check failed!", DateTime.Now);
                    return false;
                }

                if (echoResult.Result.Equals(query)) return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{date} | An exception occured during Dropbox SDK checkout.", DateTime.Now);
                Task.Run(async () => await RefreshAccessToken()).Wait(10000);
                return CheckDropboxClient();
            }

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
