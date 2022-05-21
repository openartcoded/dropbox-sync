using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.BLL.Internals
{
    internal sealed class DropboxConfiguration
    {
        public string? AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public string TokenType { get; private set; }
        public string Scopes { get; private set; }

        public bool IsValid
        {
            get
            {
                return
                    !string.IsNullOrEmpty(AccessToken) &&
                    !string.IsNullOrEmpty(RefreshToken) &&
                    !string.IsNullOrEmpty(TokenType) &&
                    !string.IsNullOrEmpty(Scopes);
            }
        }

        /// <summary>
        /// Create an instance of <see cref="DropboxConfiguration"/>
        /// </summary>
        /// <param name="refreshToken">The refresh token received by Dropbox</param>
        /// <param name="tokenType">The authorization type for Dropbox API calls</param>
        /// <param name="scopes">Authorization granted to the Dropbox application</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DropboxConfiguration(string refreshToken, string tokenType, string scopes)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentNullException(nameof(refreshToken));
            if (string.IsNullOrEmpty(tokenType)) throw new ArgumentNullException(nameof(tokenType));
            if (string.IsNullOrEmpty(scopes)) throw new ArgumentNullException(nameof(scopes));

            RefreshToken = refreshToken;
            TokenType = tokenType;
            Scopes = scopes;
        }

        /// <summary>
        /// Set the value of <see cref="AccessToken"/>
        /// </summary>
        /// <param name="accessToken">A non-nullable string</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            AccessToken = accessToken;
        }

        public override string ToString()
        {
            return
                $"Access token:\t{AccessToken}\n" +
                $"Refresh Token:\t{RefreshToken}\n" +
                $"Token type:\t{TokenType}\n" +
                $"Scopes:\t{Scopes}";
        }
    }
}
