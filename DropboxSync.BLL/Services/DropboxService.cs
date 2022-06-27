using Dropbox.Api;
using Dropbox.Api.Check;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Files;
using DropboxSync.BLL.Internals;
using DropboxSync.BLL.IServices;
using DropboxSync.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace DropboxSync.BLL.Services
{
    // TODO : Delete all the useless methods

    // File structure in the Dropbox :
    // Dossier closed : [YEAR]/[DATE-DOSSIER_NAME]/[DOSSIER_ZIP]
    // Dossier creation depends on invoices/expenses added to this dossier. On File added to dossier, first it should verify if the dossier exist
    // Files added to dossier : [YEAR]/[DATE-DOSSIER_NAME/[INVOICES/EXPENSES]/[DATE-FILE_NAME.FILE_EXTENSION]

    internal record AccessTokenResponse(string access_token, string token_type, int expires_in, string refresh_token, string scope,
        string uid, string account_id);

    internal record RefreshAccessTokenResponse(string access_token, string token_type, int expires_in);

    public class DropboxService : IDropboxService
    {
        /// <summary>
        /// API key received in the Dropbox Application Console page
        /// </summary>
        private readonly string API_KEY = Environment.GetEnvironmentVariable("DROPBOX_API_KEY") ??
            "";

        /// <summary>
        /// API Secret key received in the Dropbox Application Console page
        /// </summary>
        private readonly string API_SECRET = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
            "";

        /// <summary>
        /// The configuration Filename. If the Environment variable is null then "dropbox-sync-configuration.json" is chosen
        /// </summary>
        private readonly string CONFIG_FILE_NAME = Environment.GetEnvironmentVariable("DROPBOX_CONFIG_FILE_NAME") ??
            "dropbox-sync-configuration.json";

        /// <summary>
        /// Retrieves the declared root folder in Dropbox. If the environnement variable is null then the next folder is used
        /// <code>/OPENARTCODED</code>
        /// Be aware that the folder name must ALWAYS start with <c>/</c>
        /// </summary>
        private readonly string ROOT_FOLDER = Environment.GetEnvironmentVariable("DROPBOX_ROOT_FOLDER") ??
            "/OPENARTCODED";

        private readonly ILogger _logger;
        private readonly DropboxClient _dropboxClient;
        private DropboxConfiguration _dropboxConfig;

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

            string myDocPath = Environment.GetEnvironmentVariable("DROPBOX_CONFIG_PATH") ??
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.None);

            if (string.IsNullOrEmpty(myDocPath))
            {
                _logger.LogError("{date} | Could not retrieve path of \"My Documents\" for Windows and \"\\\" for Linux", DateTime.Now);
                Environment.Exit(1);
            }

            string configFilePath = Path.Join(myDocPath, CONFIG_FILE_NAME);

            // Verify if file exist. If it exist, deserialized it and verify if every property contains a value. If one of them is empty,
            // application stops.
            if (File.Exists(configFilePath))
            {
                _dropboxConfig = JsonConvert.DeserializeObject<DropboxConfiguration>(File.ReadAllText(configFilePath));

                if (_dropboxConfig is null)
                {
                    throw new Exception($"The dropbox deserialized Dropbox configuration returned null");
                }

                AsyncHelper.RunSync(RefreshAccessToken);

                if (!_dropboxConfig.IsValid)
                {
                    throw new Exception($"The dropbox deserialized dropbox configuration has invalid fields!\n{_dropboxConfig}");
                }

                _logger.LogInformation("{date} | The retrieved refresh token is \"{refreshToken}\"", DateTime.Now, _dropboxConfig.ToString());
            }
            // Verify if the CODE given in Env variable returns the token
            else
            {
                string? codeFromEnvironment = Environment.GetEnvironmentVariable("DROPBOX_CODE");
                if (string.IsNullOrEmpty(codeFromEnvironment))
                {
                    throw new Exception($"Could not retrieve Dropbox OAuth2 code from Environment. Please read the documentation " +
                        $"and try again!");
                }

                _dropboxConfig = AsyncHelper.RunSync(InitDropboxConfiguration);

                string configJson = JsonConvert.SerializeObject(_dropboxConfig);

                File.WriteAllText(configFilePath, configJson);
            }

            _dropboxClient = new DropboxClient(_dropboxConfig.AccessToken);

            if (!CheckDropboxClient()) throw new Exception($"An error occured during Dropbox Client checkout. Please read the precedent " +
                $"logs to understand the error");

            if (!AsyncHelper.RunSync(VerifyRootFolder)) throw new DropboxRootFolderMissingException(nameof(ROOT_FOLDER));
        }

        /// <summary>
        /// Create a file in Dropbox.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="createdAt"></param>
        /// <param name="fileRelativePath"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<DropboxSavedFile> SaveUnprocessedFileAsync(string fileName, DateTime createdAt, string fileRelativePath, FileTypes fileType)
        {
            if (string.IsNullOrEmpty(fileRelativePath)) throw new ArgumentNullException(nameof(fileRelativePath));

            string requiredDropboxFolder = GeneratedFolderPath(createdAt.Year, fileType);

            string? folderDropboxPath = await VerifyFolderExist(requiredDropboxFolder, true);
            if (string.IsNullOrEmpty(folderDropboxPath))
            {
                _logger.LogError("{date} | The folder couldn't be checked nor created!", DateTime.Now);
                throw new NullValueException(nameof(folderDropboxPath));
            }

            string dropboxFileName = GenerateDropboxFileName(fileName, createdAt);

            string dropboxDestinationPath = GenerateFileDestinationPath(requiredDropboxFolder, dropboxFileName);

            FileMetadata dropboxUploadResult = await _dropboxClient
                .Files
                .UploadAsync(new UploadArg(dropboxDestinationPath), new FileStream(fileRelativePath, FileMode.Open));

            string dropboxId = dropboxUploadResult.Id.Substring(dropboxUploadResult.Id.IndexOf(':') + 1);

            DropboxSavedFile finalOuput = new DropboxSavedFile(dropboxId, dropboxUploadResult.PathDisplay);

            return finalOuput;
        }

        /// <summary>
        /// Create an empty folder in Dropbox for given year and dossier name
        /// </summary>
        /// <param name="dossierName">The name of the dossier</param>
        /// <param name="createdAt">The dossier creation date</param>
        /// <returns><c>true</c> If folder was successfully created. <c>false</c> Otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> CreateDossierAsync(string dossierName, DateTime createdAt)
        {
            if (string.IsNullOrEmpty(dossierName)) throw new ArgumentNullException(nameof(dossierName));

            string completeFolder = GenerateDossierFolderPath(createdAt.Year, dossierName);

            CreateFolderResult createFolderResult = await _dropboxClient.Files.CreateFolderV2Async(completeFolder);

            if (!createFolderResult.Metadata.IsFolder)
            {
                _logger.LogError("{date} | Something was created but not a folder at path {dropboxPath}",
                    DateTime.Now, createFolderResult.Metadata.PathDisplay);
                return false;
            }

            _logger.LogInformation("{date} | Successfully created folder for dossier \"{dossierName}\" at path {dropboxFolderPath}",
                DateTime.Now, dossierName, createFolderResult.Metadata.PathDisplay);

            return true;
        }

        /// <summary>
        /// Delete dossier from Dropbox
        /// </summary>
        /// <param name="dossierName"></param>
        /// <param name="createdAt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> DeleteDossierAsync(string dossierName, DateTime createdAt)
        {
            if (string.IsNullOrEmpty(dossierName)) throw new ArgumentNullException(nameof(dossierName));

            string dropboxDossiersPath = GeneratedFolderPath(createdAt.Year, FileTypes.Dossiers);

            string? receivedPath = await VerifyFolderExist(dropboxDossiersPath);

            if (string.IsNullOrEmpty(receivedPath))
            {
                _logger.LogWarning("{date} | There is no Dossier at path \"{path}\"", DateTime.Now, dropboxDossiersPath);
                return false;
            }

            ListFolderResult? listFolderResult = await _dropboxClient.Files.ListFolderAsync(dropboxDossiersPath, includeMountedFolders: true);

            if (listFolderResult is null)
            {
                _logger.LogError("{date} | Listing dropbox folders at path \"{path}\" returned null!", DateTime.Now, dropboxDossiersPath);
                return false;
            }

            DeleteResult? deleteResult = await _dropboxClient.Files.DeleteV2Async($"{dropboxDossiersPath}/{dossierName}");

            if (deleteResult is null)
            {
                _logger.LogError("{date} | The deletion of folder at path \"{path}\" returned null!",
                    DateTime.Now, receivedPath);
                return false;
            }

            _logger.LogInformation("{date} | Folder successfully deleted in Dropbox at path \"{path}\"",
                DateTime.Now, deleteResult.Metadata.PathDisplay);

            return true;
        }

        public async Task<DropboxSavedFile?> SaveDossierAsync(string dossierName, string fileName, string dossierRelativePath, DateTime createdAt)
        {
            if (string.IsNullOrEmpty(dossierName)) throw new ArgumentNullException(nameof(dossierName));
            if (string.IsNullOrEmpty(dossierRelativePath)) throw new ArgumentNullException(nameof(dossierRelativePath));

            DropboxSavedFile? dropboxSaved = null;

            string dropboxDestinationPath = GenerateArchivePath();

            string? dropboxFolderPath = await VerifyFolderExist(dropboxDestinationPath, true);

            if (string.IsNullOrEmpty(dropboxFolderPath))
            {
                _logger.LogError("{date} | There is no destination folder for this dossier", DateTime.Now);
                return dropboxSaved;
            }

            string dropboxFileName = GenerateDropboxFileName(fileName, createdAt);
            string destinationPath = GenerateFileDestinationPath(dropboxDestinationPath, dropboxFileName);

            FileMetadata? dropboxUploadResult = await _dropboxClient.Files.UploadAsync(new UploadArg(destinationPath),
                        new FileStream(dossierRelativePath, FileMode.Open));

            if (dropboxUploadResult is null)
            {
                _logger.LogError("{date} | File \"{fileName}\" could not be created at path \"{path}\"",
                    DateTime.Now, fileName, dropboxFolderPath);
                return dropboxSaved;
            }

            string dropboxId = dropboxUploadResult.Id.Substring(dropboxUploadResult.Id.IndexOf(':') + 1);

            dropboxSaved = new DropboxSavedFile(dropboxId, dropboxUploadResult.PathDisplay);

            return dropboxSaved;
        }

        public async Task<DropboxMovedFile?> MoveFileAsync(string dropboxFileId, DateTime fileCreationDate, FileTypes movingFilesType,
            bool isProcess, string? dossierName = null, string? label = null)
        {
            if (string.IsNullOrEmpty(dropboxFileId)) throw new ArgumentNullException(nameof(dropboxFileId));
            if (string.IsNullOrEmpty(dossierName) && isProcess) throw new ArgumentNullException(nameof(dossierName));
            if (!string.IsNullOrEmpty(label) && movingFilesType != FileTypes.Expenses)
                throw new InvalidEnumValueException(nameof(movingFilesType));

            DropboxMovedFile? finalOutput = null;

            Metadata? metadata = await _dropboxClient.Files.GetMetadataAsync($"id:{dropboxFileId}");

            if (metadata is null)
            {
                _logger.LogError("{date} | There is no file with id \"{fileId}\" registered in Dropbox", DateTime.Now, dropboxFileId);
                return finalOutput;
            }

            if (!metadata.IsFile)
            {
                _logger.LogError("{date} | The file with ID \"{id}\" is not a file.", DateTime.Now, dropboxFileId);
                return finalOutput;
            }

            string dropboxFilePath = metadata.PathDisplay;

            if (string.IsNullOrEmpty(dropboxFilePath))
            {
                _logger.LogError("{date} | The match's path is null or an empty string!", DateTime.Now);
                return finalOutput;
            }

            RelocationResult relocationResult;
            string newPath;

            if (isProcess)
            {
                if (string.IsNullOrEmpty(dossierName)) throw new NullValueException(nameof(dossierName));

                if (string.IsNullOrEmpty(label))
                {
                    newPath = GenerateDossierFolderPath(fileCreationDate.Year, dossierName, movingFilesType);
                }
                else
                {
                    newPath = GenerateLabeledExpenseDestinationPath(fileCreationDate.Year, label, dossierName);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(label))
                {
                    newPath = GeneratedFolderPath(fileCreationDate.Year, movingFilesType);
                }
                else
                {
                    newPath = GenerateLabeledExpenseDestinationPath(fileCreationDate.Year, label);
                }
            }

            string? verifiedPath = await VerifyFolderExist(newPath, true);

            if (string.IsNullOrEmpty(verifiedPath))
            {
                _logger.LogError("{date} | No folder exist at path \"{path}\" and it couldn't be created!", DateTime.Now, newPath);
                throw new NullValueException(nameof(verifiedPath));
            }

            string toPath = GenerateFileDestinationPath(verifiedPath, metadata.Name);

            relocationResult = await _dropboxClient.Files.MoveV2Async(dropboxFilePath, toPath, allowSharedFolder: true, allowOwnershipTransfer: true);

            if (relocationResult is null)
            {
                _logger.LogError("{date} | The relocation failed, returning a \"null\"", DateTime.Now);
                throw new NullValueException(nameof(relocationResult));
            }

            finalOutput = new DropboxMovedFile(dropboxFilePath, toPath);

            return finalOutput;
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

        private async Task<DropboxConfiguration> InitDropboxConfiguration()
        {
            string? code = Environment.GetEnvironmentVariable("DROPBOX_CODE");
            if (string.IsNullOrEmpty(code))
            {
                throw new Exception($"Please read the documentation and provided a valid Dropbox OAuth2 code as an environment variable");
            }

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
                throw new Exception($"{typeof(HttpResponseMessage)} returned null");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP Request failed with status : \"{response.StatusCode}\".");
            }

            if (response.Content is null)
            {
                throw new Exception($"HTTP Request content \"{nameof(response.Content)}\" is null");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("{date} | HTTP Request response received : {response}", DateTime.Now, jsonResponse);

            AccessTokenResponse accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(jsonResponse);
            if (accessTokenResponse is null)
            {
                throw new Exception($"Deserialized object of type \"{typeof(AccessTokenResponse)}\" is null");
            }

            DropboxConfiguration dropboxConfiguration = new DropboxConfiguration(accessTokenResponse.refresh_token,
                accessTokenResponse.token_type, accessTokenResponse.scope);

            dropboxConfiguration.SetAccessToken(accessTokenResponse.access_token);

            if (!dropboxConfiguration.IsValid)
            {
                throw new Exception($"One of {nameof(dropboxConfiguration)}'s properties is invalid.\n{dropboxConfiguration}");
            }

            return dropboxConfiguration;
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

            _dropboxConfig.SetAccessToken(accessTokenResponse.access_token);
        }

        /// <summary>
        /// Retrieves a new access token from Dropbox API. If the <see cref="RefreshToken"/> is <c>null</c> or <c>empty</c> or environnement variable 
        /// containing the refresh token at <c>DROPBOX_REFRESH_TOKEN</c> is also <c>null</c> or <c>empty</c> then logger displays the error in the 
        /// console and break the method.
        /// </summary>
        /// <returns></returns>
        private async Task RefreshAccessToken()
        {
            if (string.IsNullOrEmpty(_dropboxConfig.RefreshToken))
            {
                throw new Exception($"{nameof(_dropboxConfig.RefreshToken)} is null!");
            }

            using HttpClient httpClient = new HttpClient();

            string authorizationScheme = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{API_KEY}:{API_SECRET}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationScheme);

            KeyValuePair<string, string>[] data = new[]
            {
                new KeyValuePair<string, string>("grant_type","refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _dropboxConfig.RefreshToken)
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

            _dropboxConfig.SetAccessToken(refreshAccessTokenResponse.access_token);
        }

        /// <summary>
        /// Generate a path depending on the file type
        /// </summary>
        /// <param name="createdAt">The file's creation date</param>
        /// <param name="fileType">The type of File generated</param>
        /// <returns>
        /// Dropbox's complete path <c> ROOT_FOLDER/YEAR/UNPROCESSED/FILETYPE </c> for Invoices and Expenses and
        /// <c> ROOT_FOLDER/YEAR/DOSSIERS </c> for Dossier
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GeneratedFolderPath(int year, FileTypes fileType)
        {
            if (year <= 0 || year > DateTime.Now.Year) throw new ArgumentOutOfRangeException(nameof(year));

            return fileType switch
            {
                FileTypes.Invoices or FileTypes.Expenses =>
                    string.Join('/', ROOT_FOLDER, year.ToString(), "UNPROCESSED", fileType.ToString().ToUpper()),
                FileTypes.Documents =>
                    string.Join('/', ROOT_FOLDER, year.ToString(), fileType.ToString().ToUpper()),
                FileTypes.Dossiers =>
                    string.Join('/', ROOT_FOLDER, year.ToString(), fileType.ToString().ToUpper()),
                _ =>
                    throw new InvalidEnumValueException(nameof(fileType)),
            };
        }

        /// <summary>
        /// Generate the path to a dossier in Dropbox
        /// </summary>
        /// <param name="year">Dossier creation year</param>
        /// <param name="dossierName">Dossier name</param>
        /// <returns>Dropbox absolute path to the folder</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private string GenerateDossierFolderPath(int year, string dossierName)
        {
            dossierName = dossierName.Trim();

            if (year <= 0 || year > DateTime.Now.Year) throw new ArgumentOutOfRangeException(nameof(year));
            if (string.IsNullOrEmpty(dossierName)) throw new ArgumentNullException(nameof(dossierName));

            return string.Join('/', ROOT_FOLDER, year.ToString(), FileTypes.Dossiers.ToString().ToUpper(), dossierName);
        }

        /// <summary>
        /// Generate the path to a dossier's invoice or expense folder in Dropbox
        /// </summary>
        /// <param name="year">File creation year</param>
        /// <param name="dossierName">Dossier name</param>
        /// <param name="fileType">File type</param>
        /// <returns>Dropbox's absolute path to the <paramref name="fileType"/> folder in dossier named <paramref name="dossierName"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEnumValueException"></exception>
        private string GenerateDossierFolderPath(int year, string dossierName, FileTypes fileType)
        {
            dossierName = dossierName.Trim();

            if (year <= 0 | year > DateTime.Now.Year) throw new ArgumentOutOfRangeException(nameof(year));
            if (string.IsNullOrEmpty(dossierName)) throw new ArgumentNullException(nameof(dossierName));
            if (fileType == FileTypes.Dossiers) throw new InvalidEnumValueException(nameof(FileTypes));

            return string.Join('/',
                ROOT_FOLDER,
                year.ToString(),
                FileTypes.Dossiers.ToString().ToUpper(),
                dossierName,
                fileType.ToString().ToUpper());
        }

        /// <summary>
        /// Generate a name for the file to save in Dropbox. The name is composed of the the date and the filename seperated by
        /// <paramref name="seperator"/>. If at date <c>2022-10-22</c> at <c>18:42</c> a file with name <c>MyFilesName.pdf</c>
        /// is created, then the generated name would look like this.
        /// <code>
        /// 2022.10.22 1842-MyFilesName.pdf
        /// </code>
        /// </summary>
        /// <param name="fileName">File's complete name. The filename must respect the Regex</param>
        /// <param name="createdAt"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidFileNameException"></exception>
        private string GenerateDropboxFileName(string fileName, DateTime createdAt, char seperator = '-')
        {
            fileName = fileName.Trim();

            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (fileName.StringMatchFileRegEx()) throw new InvalidFileNameException(nameof(fileName));
            if (createdAt > DateTime.Now) throw new ArgumentOutOfRangeException(nameof(DateTime));

            return string.Join(seperator, createdAt.ToString("yyyy.MM.dd HHmm"), fileName);
        }

        /// <summary>
        /// Generate a Unix file path
        /// </summary>
        /// <param name="destinationFolderPath">Dropbox folder's complete destination path</param>
        /// <param name="fileName">The complete filename</param>
        /// <returns>A Unix formatted path</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GenerateFileDestinationPath(string destinationFolderPath, string fileName)
        {
            destinationFolderPath = destinationFolderPath.Trim();
            fileName = fileName.Trim();

            if (string.IsNullOrEmpty(destinationFolderPath)) throw new ArgumentNullException(nameof(destinationFolderPath));
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            return string.Join('/', destinationFolderPath, fileName);
        }

        /// <summary>
        /// Generate a destination path for expense based on its label
        /// </summary>
        /// <param name="year">Expense year of creation</param>
        /// <param name="label">Expense's label</param>
        /// <param name="dossierName">the destination dossier. If it is null or empty, the expense is sent to
        /// <c>UNPROCESSED</c> directory otherwise it is sent to the dossier</param>
        /// <returns>A Unix type folder destination path in Dropbox</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private string GenerateLabeledExpenseDestinationPath(int year, string label, string? dossierName = null)
        {
            if (year < 0) throw new IndexOutOfRangeException(nameof(year));
            if (string.IsNullOrEmpty(label)) throw new ArgumentNullException(nameof(label));

            if (string.IsNullOrEmpty(dossierName))
            {
                return string.Join('/',
                    ROOT_FOLDER,
                    year.ToString(),
                    "UNPROCESSED",
                    FileTypes.Expenses.ToString().ToUpper(),
                    label.ToUpper());
            }
            else
            {
                return string.Join('/',
                    ROOT_FOLDER,
                    year.ToString(),
                    FileTypes.Dossiers.ToString().ToUpper(),
                    dossierName,
                    FileTypes.Expenses.ToString().ToUpper(),
                    label.ToUpper());
            }
        }

        /// Generate a Unix file path to the archives in the next format
        /// <code>
        /// /ROOT_FOLDER/ARCHIVES
        /// </code>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GenerateArchivePath()
        {
            return string.Join('/', ROOT_FOLDER, "ARCHIVES");
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
                    if (file.IsFolder && file.PathDisplay.Equals(ROOT_FOLDER))
                    {
                        _logger.LogInformation("{date} | Folder {name} exist.", DateTime.Now, ROOT_FOLDER);
                        return true;
                    }
                }
            }
            while (listFolderResult.HasMore);

            CreateFolderResult createFolderResult = await _dropboxClient.Files.CreateFolderV2Async(ROOT_FOLDER);

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

        /// <summary>
        /// Verify if the folder at full dropbox path <paramref name="folderFullPath"/> exist in Dropbox. If <paramref name="createIfDontExist"/>
        /// is <c>true</c> then the folder is created.
        /// </summary>
        /// <param name="folderFullPath">
        /// Dropbox's folder full path starting from the root. For example
        /// <code>
        /// ROOT_FOLDER/2022/UNPROCESSED/INVOICES
        /// </code>
        /// </param>
        /// <param name="createIfDontExist">If <c>true</c>, creates the folder at the researched destination</param>
        /// <returns>
        /// <c>null</c> if listing at path <see cref="ROOT_FOLDER"/> failed or if folder isn't found and <paramref name="createIfDontExist"/> is false.
        /// Otherwise returns the Dropbox's required folder's path.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<string?> VerifyFolderExist(string folderFullPath, bool createIfDontExist = false)
        {
            if (string.IsNullOrEmpty(folderFullPath)) throw new ArgumentNullException(nameof(folderFullPath));

            ListFolderResult listFolderResult = await _dropboxClient.Files.ListFolderAsync($"/{ROOT_FOLDER}", recursive: true);
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
                EchoResult? echoResult = AsyncHelper.RunSync(() => _dropboxClient.Check.UserAsync(query));

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
                return false;
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
