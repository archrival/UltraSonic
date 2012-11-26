using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UltraSonic.Properties;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PopulateSettings()
        {
            Username = Settings.Default.Username;
            Password = Settings.Default.Password;
            ServerUrl = Settings.Default.ServerUrl;
            ProxyUsername = Settings.Default.ProxyUsername;
            ProxyPassword = Settings.Default.ProxyPassword;
            ProxyServer = Settings.Default.ProxyServer;
            ProxyPort = Settings.Default.ProxyPort;
            UseProxy = Settings.Default.UseProxy;
            _maxSearchResults = Settings.Default.MaxSearchResults;
            _albumListMax = Settings.Default.AlbumListMax;
            _throttle = Settings.Default.Throttle;
            _cacheDownloadLimit = Settings.Default.CacheDownloadLimit;
            _cacheDirectory = string.IsNullOrWhiteSpace(Settings.Default.CacheDirectory) ? Path.Combine(Path.Combine(_roamingPath, AppName), "Cache") : Settings.Default.CacheDirectory;
            _useDiskCache = Settings.Default.UseDiskCache;
            _nowPlayingInterval = Settings.Default.NowPlayingInterval;
            _chatMessagesInterval = Settings.Default.ChatMessagesInterval;
            _serverHash = StaticMethods.CalculateSha256(ServerUrl, Encoding.Unicode);
            _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
            _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");
            _playbackFollowsCursor = Settings.Default.PlaybackFollowsCursor;
            _currentPlaylist = Settings.Default.CurrentPlaylist ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(ServerUrl))
            {
                if (!Directory.Exists(_musicCacheDirectoryName))
                    Directory.CreateDirectory(_musicCacheDirectoryName);

                if (!Directory.Exists(_coverArtCacheDirectoryName))
                    Directory.CreateDirectory(_coverArtCacheDirectoryName);
            }

            PopulateSearchResultItemComboBox();
            PopulateMaxBitrateComboBox();
            PopulateAlbumListMaxComboBox();
            PopulateNowPlayingIntervalComboBox();
            PopulateChatMessagesIntervalComboBox();
            PopulateCacheDownloadLimitComboBox();
            PopulateThrottleComboBox();

            SettingsUseProxyCheckbox.IsChecked = UseProxy;
            SettingsUsernameTextBox.Text = Username;
            SettingsPasswordPasswordBox.Password = Password;
            SettingsServerAddressTextBox.Text = ServerUrl;
            SettingsUseProxyCheckbox.IsChecked = UseProxy;
            SettingsProxyServerAddressTextBox.Text = ProxyServer;
            SettingsProxyServerPortTextBox.Text = ProxyPort.ToString(CultureInfo.InvariantCulture);
            SettingsProxyServerUsernameTextBox.Text = ProxyUsername;
            SettingsProxyServerPasswordTextBox.Password = ProxyPassword;
            CacheDirectoryTextBox.Text = _cacheDirectory;
            UseDiskCacheCheckBox.IsChecked = _useDiskCache;
            PlaybackFollowsCursorCheckBox.IsChecked = _playbackFollowsCursor;

            SetProxyEntryVisibility(UseProxy);
            SetUseDiskCacheVisibility(_useDiskCache);
        }

        private void PopulatePlaylist()
        {
            Dispatcher.Invoke(() =>
                                  {
                                      ObservableCollection<TrackItem> playlistTrackItems;
                                      XmlSerializer xmlSerializer = new XmlSerializer(_playlistTrackItems.GetType());

                                      using (TextReader reader = new StringReader(_currentPlaylist))
                                          playlistTrackItems = xmlSerializer.Deserialize(reader) as ObservableCollection<TrackItem>;

                                      if (playlistTrackItems != null)
                                          foreach (TrackItem trackItem in playlistTrackItems)
                                              AddTrackItemToPlaylist(trackItem);
                                  });
        }

        private void PopulateSearchResultItemComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 2500; i++)
                listData.Add(i);

            MaxSearchResultsComboBox.ItemsSource = listData;
            MaxSearchResultsComboBox.SelectedItem = _maxSearchResults;
        }

        private void PopulateAlbumListMaxComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 500; i++)
                listData.Add(i);

            AlbumListMaxComboBox.ItemsSource = listData;
            AlbumListMaxComboBox.SelectedItem = _albumListMax;
        }

        private void PopulateNowPlayingIntervalComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 300; i++)
                listData.Add(i);

            NowPlayingIntervalComboBox.ItemsSource = listData;
            NowPlayingIntervalComboBox.SelectedItem = _nowPlayingInterval;
        }

        private void PopulateChatMessagesIntervalComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 300; i++)
                listData.Add(i);

            ChatMessagesIntervalComboBox.ItemsSource = listData;
            ChatMessagesIntervalComboBox.SelectedItem = _chatMessagesInterval;
        }

        private void PopulateMaxBitrateComboBox()
        {
            List<int> listData = new List<int> { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 };

            MaxBitrateComboBox.ItemsSource = listData;
            MaxBitrateComboBox.SelectedItem = _maxBitrate;
        }

        private void PopulateCacheDownloadLimitComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 0; i <= 300; i++)
                listData.Add(i);

            CacheDownloadLimitComboBox.ItemsSource = listData;
            CacheDownloadLimitComboBox.SelectedItem = _cacheDownloadLimit;
        }

        private void PopulateThrottleComboBox()
        {
            List<int> listData = new List<int>();
            for (int i = 1; i <= 1000; i++)
                listData.Add(i);

            ThrottleComboBox.ItemsSource = listData;
            ThrottleComboBox.SelectedItem = _throttle;
        }

        private void SetProxyEntryVisibility(bool isChecked)
        {
            Dispatcher.Invoke(() =>
            {
                SettingsProxyServerAddressTextBox.IsEnabled = isChecked;
                SettingsProxyServerPasswordTextBox.IsEnabled = isChecked;
                SettingsProxyServerPortTextBox.IsEnabled = isChecked;
                SettingsProxyServerUsernameTextBox.IsEnabled = isChecked;
            });
        }

        private void SetUseDiskCacheVisibility(bool isChecked)
        {
            Dispatcher.Invoke(() =>
            {
                CacheDirectoryTextBox.IsEnabled = isChecked;
            });
        }
    }
}
