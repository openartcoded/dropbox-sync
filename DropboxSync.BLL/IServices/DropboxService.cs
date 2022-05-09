using Dropbox.Api;
using Dropbox.Api.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.IServices
{
    // TODO : Test the dropbox StartAsync
    // TODO : Delete all the useless methods
    // TODO : Add CRUD operations
    public class DropboxService
    {
        private const string LOOPBACK_HOST = "http://localhost:52475/";

        private readonly ILogger _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _accessToken;
        private readonly Uri _redirectUrl = new Uri(LOOPBACK_HOST + "authorize");
        private readonly Uri _jsRedirectUrl = new Uri(LOOPBACK_HOST + "token");

        public string AccessToken { get; private set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

        public DropboxService(ILogger<DropboxService> logger)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _apiKey = Environment.GetEnvironmentVariable("DROPBOX_API_KEY")
                ?? throw new NullReferenceException("No api key for Dropbox has been registered in the environnement");
            _apiSecret = Environment.GetEnvironmentVariable("DROPBOX_API_SECRET") ??
                throw new NullReferenceException("No api secret for dropbox has been registered in the environnement");
            _accessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN")
                ?? throw new NullReferenceException(nameof(_accessToken));

            AccessToken = Environment.GetEnvironmentVariable("DROPBOX_ACCESS_TOKEN")
                ?? Task.Run(() => GetDropboxAccessToken()).Result
                ?? throw new NullReferenceException("No access token was obtainable!");
            RefreshToken = Environment.GetEnvironmentVariable("DROPBOX_REFRESH_TOKEN")
                ?? Task.Run(() => GetDropboxRefreshToken()).Result
                ?? throw new NullReferenceException("No refresh token was obtainable!");
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

        private async Task<string?> GetDropboxAccessToken()
        {
            string state = Guid.NewGuid().ToString("N");
            Uri authorizeUrl = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, _apiKey, _redirectUrl, state: state,
                tokenAccessType: TokenAccessType.Offline);

            HttpListener http = new HttpListener();
            http.Prefixes.Add(LOOPBACK_HOST);

            http.Start();

            System.Diagnostics.Process.Start(authorizeUrl.ToString());

            await HandleOAuth2Redirect(http);

            Uri redirectUrl = await HandleJSRedirect(http);

            OAuth2Response? tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(redirectUrl, _apiKey, _apiSecret,
                redirectUrl.ToString(), state);

            if (tokenResult is null) throw new NullReferenceException(nameof(tokenResult));

            AccessToken = tokenResult.AccessToken;
            RefreshToken = tokenResult.RefreshToken;

            string? uid = tokenResult.Uid;

            if (AccessToken is null) throw new NullReferenceException(nameof(AccessToken));
            if (RefreshToken is null) throw new NullReferenceException(nameof(RefreshToken));
            if (string.IsNullOrEmpty(uid)) throw new NullReferenceException(nameof(uid));

            Environment.SetEnvironmentVariable("DROPBOX_ACCESS_TOKEN", AccessToken);
            Environment.SetEnvironmentVariable("DROPBOX_REFRESH_TOKEN", RefreshToken);
            //Environment.SetEnvironmentVariable("DROPBOX_UID", uid);

            http.Stop();

            return uid;
        }

        private string? GetDropboxRefreshToken()
        {
            throw new NotImplementedException();
        }

        private async Task HandleOAuth2Redirect(HttpListener http)
        {
            if (http is null) throw new ArgumentNullException(nameof(http));

            HttpListenerContext? context = await http.GetContextAsync();

            if (context is null) throw new NullReferenceException(nameof(context));

            while (context.Request?.Url?.AbsolutePath != _redirectUrl.AbsolutePath)
            {
                context = await http.GetContextAsync();
            }

            context.Response.ContentType = "text/html";

            using (FileStream? file = File.OpenRead("index.html"))
            {
                file.CopyTo(context.Response.OutputStream);
            }

            context.Response.OutputStream.Close();
        }

        private async Task<Uri> HandleJSRedirect(HttpListener http)
        {
            if (http is null) throw new ArgumentNullException(nameof(http));

            HttpListenerContext? context = await http.GetContextAsync();

            if (context is null) throw new NullReferenceException(nameof(context));

            while (context.Request?.Url?.AbsolutePath != _redirectUrl.AbsolutePath)
            {
                context = await http.GetContextAsync();
            }

            Uri? redirectUrl = new Uri(context.Request?
                .QueryString["url_with_fragment"] ?? throw new NullReferenceException(nameof(context.Response)))
                ?? throw new NullReferenceException(nameof(context.Request));

            return redirectUrl;
        }
    }
}
