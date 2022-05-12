using DropboxSync.BLL.Dtos;
using DropboxSync.BLL.IServices;
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
        private readonly string API_BACKEND_URL = Environment.GetEnvironmentVariable("API_BACKEND_URL")
            ?? "http://localhost:9000";
        private readonly string API_CLIENT_ID = Environment.GetEnvironmentVariable("API_BACKEND_ID")
            ?? "service-account-download";
        private readonly string API_CLIENT_SECRET = Environment.GetEnvironmentVariable("API_CLIENT_SECRET")
            ?? "duzp0kzwDHSS2nSO46P3GBGsNnQbx5L3";
        private readonly string API_TOKEN_URL = Environment.GetEnvironmentVariable("API_TOKEN_URL")
            ?? "http://localhost:8000/realm/Artcoded/protocol/openid-connect/token";
        //?? "http://localhost:8080/realms/Artcoded/protocol/openid-connect/token";
        private readonly string FILE_DOWNLOAD_DIR = Environment.GetEnvironmentVariable("FILE_DOWNLOAD_DIR")
            ?? @"\data";

        private const string CONTENT_TYPE = "application/x-www-form-urlencoded";

        private readonly HttpClient _httpClient;

        public string Token { get; private set; } = string.Empty;

        public FileService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> DownloadFile(string fileId)
        {
            if (string.IsNullOrEmpty(Token))
            {
                if (!await GetToken()) throw new Exception("The API Token could not be retrieved!");
            }

            string fileDownloadUrl = $"{API_BACKEND_URL}/api/resource/download/id={fileId}";

            await _httpClient.GetAsync(fileDownloadUrl);

            HttpResponseMessage response = await _httpClient.GetAsync(API_BACKEND_URL + fileId);

            if (response is null) throw new ArgumentNullException(nameof(response));

            if (!response.IsSuccessStatusCode) return false;



            return true;
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

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, API_TOKEN_URL)
            {
                Content = new FormUrlEncodedContent(parameters),
                Method = HttpMethod.Post,
            };

            var response = await _httpClient.PostAsync(API_TOKEN_URL, new FormUrlEncodedContent(parameters));

            if (response is null) throw new NullReferenceException(nameof(response));

            if (!response.IsSuccessStatusCode) return false;

            JObject jObj = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (jObj is null) return false;

            JToken? jToken = jObj["access_token"];
            if (jToken is null) return false;

            string token = jToken.ToString();
            if (string.IsNullOrEmpty(token)) return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return true;
        }
    }
}
