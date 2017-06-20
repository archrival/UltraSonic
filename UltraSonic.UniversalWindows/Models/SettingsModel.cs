using Subsonic.Client.Tasks;

namespace UltraSonic.Models
{
    public class SettingsModel : ObservableObject
    {
        private string _username;
        private string _password;
        private string _serverUrl;
        private bool _useProxy;
        private string _proxyServer;
        private int _proxyPort;
        private string _proxyUsername;
        private string _proxyPassword;
        private int _maxBitrate;
        private int _maxAlbumResults;
        private int _maxSongResults;

        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password)
                    return;

                _password = value;
                OnPropertyChanged();
            }
        }

        public string ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                if (value == _serverUrl)
                    return;

                _serverUrl = value;
                OnPropertyChanged();
            }
        }
        
        public bool UseProxy
        {
            get { return _useProxy; }
            set
            {
                if (value == _useProxy)
                    return;

                _useProxy = value;
                OnPropertyChanged();
            }
        }

        public string ProxyServer
        {
            get { return _proxyServer; }
            set
            {
                if (value == _proxyServer)
                    return;

                _proxyServer = value;
                OnPropertyChanged();
            }
        }

        public string ProxyUsername
        {
            get { return _proxyUsername; }
            set
            {
                if (value == _proxyUsername)
                    return;

                _proxyUsername = value;
                OnPropertyChanged();
            }
        }

        public string ProxyPassword
        {
            get { return _proxyPassword; }
            set
            {
                if (value == _proxyPassword)
                    return;

                _proxyPassword = value;
                OnPropertyChanged();
            }
        }

        public int ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                if (value == _proxyPort)
                    return;

                _proxyPort = value;
                OnPropertyChanged();
            }
        }

        public int MaxBitrate
        {
            get { return _maxBitrate; }
            set
            {
                if (value == _maxBitrate)
                    return;

                _maxBitrate = value;
                OnPropertyChanged();
            }
        }

        public int MaxAlbumResults
        {
            get { return _maxAlbumResults; }
            set
            {
                if (value == _maxAlbumResults)
                    return;

                _maxAlbumResults = value;
                OnPropertyChanged();
            }
        }
        public int MaxSongResults
        {
            get { return _maxSongResults; }
            set
            {
                if (value == _maxSongResults)
                    return;

                _maxSongResults = value;
                OnPropertyChanged();
            }
        }
    }
}
