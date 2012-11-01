using System;

namespace Subsonic.Rest.Api
{
    public partial class SubsonicApi
    {
        private const string DefaultUserAgent = "Subsonic.Rest.Api";
        private readonly Version _apiVersion = Version.Parse("1.8.0");

        public SubsonicApi(Uri serverUrl, string userName, string password, string proxyPassword = null, string proxyUserName = null, int proxyPort = 0, string proxyServer = null)
        {
            ProxyServer = proxyServer;
            ProxyPort = proxyPort;
            ProxyUserName = proxyUserName;
            ProxyPassword = proxyPassword;
            ServerUrl = serverUrl;
            UserName = userName;
            Password = password;
            UserAgent = DefaultUserAgent;
            EncodePasswords = false;
        }

        /// <summary>
        /// Subsonic API version supported by this library.
        /// </summary>
        public Version ApiVersion
        {
            get { return _apiVersion; }
        }

        /// <summary>
        /// URL of the Subsonic server.
        /// </summary>
        private Uri ServerUrl { get; set; }

        /// <summary>
        /// Username of the Subsonic user.
        /// </summary>
        private string UserName { get; set; }

        /// <summary>
        /// Password of the Subsonic user.
        /// </summary>
        private string Password { get; set; }

        /// <summary>
        /// Proxy server to use for HTTP/HTTPS communication.
        /// </summary>
        private string ProxyServer { get; set; }

        /// <summary>
        /// Port number of the proxy server.
        /// </summary>
        private int ProxyPort { get; set; }

        /// <summary>
        /// Username of the proxy server if the proxy is authenticated.
        /// </summary>
        private string ProxyUserName { get; set; }

        /// <summary>
        /// Password of the proxy server user.
        /// </summary>
        private string ProxyPassword { get; set; }

        /// <summary>
        /// User agent to use for HTTP/HTTPS communication. [Default = Subsonic.Rest.Api]
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// If we should encode passwords when passed as a parameter. [Default = false]
        /// </summary>
        public bool EncodePasswords { get; set; }

        /// <summary>
        /// Subsonic API version supported by the Subsonic server.
        /// </summary>
        public Version ServerApiVersion { get; set; }
    }
}