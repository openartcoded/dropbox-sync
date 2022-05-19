﻿using DropboxSync.BLL.Dtos;
using DropboxSync.Helpers;
using DropboxSync.BLL.IServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Services
{
    public class FileService : IFileService
    {
        /// <summary>
        /// Represent the api-backend service's URL. If no environnement variable named API_BACKEND_URL exists, string
        /// "http://localhost:9000" is used
        /// </summary>
        private readonly string API_BACKEND_URL = Environment.GetEnvironmentVariable("API_BACKEND_URL")
            ?? "http://localhost:9000";

        private readonly string API_CLIENT_ID = Environment.GetEnvironmentVariable("API_BACKEND_ID")
            ?? "service-account-download";

        private readonly string API_CLIENT_SECRET = Environment.GetEnvironmentVariable("API_CLIENT_SECRET")
            ?? "duzp0kzwDHSS2nSO46P3GBGsNnQbx5L3";

        private readonly string API_TOKEN_URL = Environment.GetEnvironmentVariable("API_TOKEN_URL")
            ?? "http://localhost:8080/realms/Artcoded/protocol/openid-connect/token";

        private readonly string FILE_DOWNLOAD_DIR = Environment.GetEnvironmentVariable("FILE_DOWNLOAD_DIR")
            ?? "Data";

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;


        public FileService(ILogger<FileService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string filesDirectory = Path.Join(appData, FILE_DOWNLOAD_DIR);

            if (!Directory.Exists(filesDirectory))
            {
                Directory.CreateDirectory(filesDirectory);
            }
        }

        /// <summary>
        /// Download the file from API and save it locally. Finally returns an <see cref="SavedFile"/> object containing needed data
        /// </summary>
        /// <param name="fileId">The ID of the file to download</param>
        /// <returns><see cref="SavedFile"/> object if download and save was successfull. <c>null</c> Otherwise</returns>
        public async Task<SavedFile?> DownloadFile(string fileId)
        {
            if (string.IsNullOrEmpty(fileId)) throw new ArgumentNullException(nameof(fileId));

            SavedFile? finalOutput = null;

            if (!await GetToken())
            {
                _logger.LogError("{date} | Could not retrieve Access Token to the API.", DateTime.Now);
                return finalOutput;
            }

            string fileDownloadUrl = $"{API_BACKEND_URL}/api/resource/download?id={fileId}";

            HttpResponseMessage response = await _httpClient.GetAsync(fileDownloadUrl);

            if (response is null)
            {
                _logger.LogError("{date} | Something went wrong on api call to route \"{fileDownloadUrl}\"",
                    DateTime.Now, fileDownloadUrl);
                return finalOutput;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("{date} | The API call was unsuccesfull. Reponse status is \"{responseCode}\"",
                    DateTime.Now, response.StatusCode);

                return finalOutput;
            }

            if (response.Content is null)
            {
                _logger.LogError("{date} | Response content is null!", DateTime.Now);
                return finalOutput;
            }


            byte[] fileData = await response.Content.ReadAsByteArrayAsync();

            if (fileData is null)
            {
                _logger.LogError("{date} | Byte array from file is null", DateTime.Now);
                return finalOutput;
            }

            string? fileName = response.Content.Headers?.ContentDisposition?.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogError("{date} | The file name for file with ID \"{fileId}\" could not be retrieved!",
                    DateTime.Now, fileId);
                return finalOutput;
            }

            fileName = fileName.Replace("\"", "");
            string fileExtension = fileName.Split('.').Last();

            string? contentType = response.Content.Headers?.ContentType?.MediaType;

            if (string.IsNullOrEmpty(contentType))
            {
                _logger.LogError("{date} | The content type for file with ID \"{fileId}\" could not be retrieved!",
                    DateTime.Now, fileId);
                return finalOutput;
            }

            string filePath = Path.Combine(FILE_DOWNLOAD_DIR, string.Join('-', fileId, fileName));

            await File.WriteAllBytesAsync(filePath, fileData);
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                _logger.LogError("{date} | File at path \"{filePath}\" doesn't exist!", DateTime.Now, filePath);
                return finalOutput;
            }

            long fileSize = fileInfo.Length;

            finalOutput = new SavedFile(filePath, contentType, fileName, fileSize, fileExtension);

            _logger.LogInformation("{date} | File saved at {filepath}", DateTime.Now, filePath);

            return finalOutput;
        }

        public bool Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            string filePath = Path.Combine(FILE_DOWNLOAD_DIR, fileName);
            File.Delete(filePath);

            return !File.Exists(filePath);
        }

        private async Task<bool> GetToken()
        {
            ApiAuthenticationDto apiAuthentication = new ApiAuthenticationDto()
            {
                ClientSecret = API_CLIENT_SECRET,
                ClientId = API_CLIENT_ID,
            };

            var parameters = new Dictionary<string, string>();
            parameters.Add("grant_type", apiAuthentication.GrantType);
            parameters.Add("client_id", apiAuthentication.ClientId);
            parameters.Add("client_secret", apiAuthentication.ClientSecret);

            var response = await _httpClient.PostAsync(API_TOKEN_URL, new FormUrlEncodedContent(parameters));

            if (response is null) throw new NullReferenceException(nameof(response));

            if (!response.IsSuccessStatusCode) return false;

            ApiAuthenticationSuccessDto apiAuthenticationSuccess = JsonConvert
                .DeserializeObject<ApiAuthenticationSuccessDto>(await response.Content.ReadAsStringAsync());

            if (apiAuthenticationSuccess is null)
            {
                _logger.LogError("{datetime} | Couldn't deserialize the response with value \"{responseValue}\"!",
                    DateTime.Now, await response.Content.ReadAsStringAsync());
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                apiAuthenticationSuccess.AccessToken);

            return true;
        }
    }
}
