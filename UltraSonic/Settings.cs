using Subsonic.Client.Common.Items;
using System;
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
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

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
            _cacheDirectory = string.IsNullOrWhiteSpace(Settings.Default.CacheDirectory) ? Path.Combine(Path.Combine(_roamingPath, AppName), "Cache") : Settings.Default.CacheDirectory;
            _nowPlayingInterval = Settings.Default.NowPlayingInterval;
            _chatMessagesInterval = Settings.Default.ChatMessagesInterval;
            _serverHash = StaticMethods.CalculateSha256(ServerUrl, Encoding.Unicode);
            _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
            _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");
            _playbackFollowsCursor = Settings.Default.PlaybackFollowsCursor;
            _currentPlaylist = Settings.Default.CurrentPlaylist ?? string.Empty;
            _currentPlaybackList = Settings.Default.CurrentPlaybackList ?? string.Empty;
            _albumArtSize = Settings.Default.AlbumArtSize;
            _saveWorkingPlaylist = Settings.Default.SaveWorkingPlaylist;
            _savePlaybackList = Settings.Default.SavePlaybackList;
            _showAlbumArt = Settings.Default.ShowAlbumArt;
            _cachePlaylistTracks = Settings.Default.CachePlaylistTracks;

            if (!Enum.TryParse(Settings.Default.DoubleClickBehavior, out _doubleClickBehavior)) _doubleClickBehavior = DoubleClickBehavior.Add;
            if (!Enum.TryParse(Settings.Default.AlbumPlayButtonBehavior, out _albumPlayButtonBehavior)) _albumPlayButtonBehavior = AlbumPlayButtonBehavior.Ask;

            if (!string.IsNullOrWhiteSpace(ServerUrl))
            {
                if (!Directory.Exists(_musicCacheDirectoryName))
                    Directory.CreateDirectory(_musicCacheDirectoryName);

                if (!Directory.Exists(_coverArtCacheDirectoryName))
                    Directory.CreateDirectory(_coverArtCacheDirectoryName);
            }

            PopulateMaxBitrateComboBox();
            PopulateDoubleClickComboBox();
            PopulateAlbumPlayBehaviorComboBox();

            SettingsUseProxyCheckbox.IsChecked = UseProxy;
            SettingsUsernameTextBox.Text = Username;
            SettingsPasswordPasswordBox.Password = Password;
            SettingsServerAddressTextBox.Text = ServerUrl;
            SettingsUseProxyCheckbox.IsChecked = UseProxy;
            SettingsProxyServerAddressTextBox.Text = ProxyServer;
            SettingsProxyServerPortTextBox.Text = ProxyPort.ToString(CultureInfo.InvariantCulture);
            SettingsProxyServerUsernameTextBox.Text = ProxyUsername;
            SettingsProxyServerPasswordTextBox.Password = ProxyPassword;
            AlbumListMaxTextBox.Text = _albumListMax.ToString(CultureInfo.InvariantCulture);
            AlbumArtSizeTextBox.Text = _albumArtSize.ToString(CultureInfo.InvariantCulture);
            ThrottleTextBox.Text = _throttle.ToString(CultureInfo.InvariantCulture);
            MaxSearchResultsTextBox.Text = _maxSearchResults.ToString(CultureInfo.InvariantCulture);
            NowPlayingIntervalTextBox.Text = _nowPlayingInterval.ToString(CultureInfo.InvariantCulture);
            ChatMessagesIntervalTextBox.Text = _chatMessagesInterval.ToString(CultureInfo.InvariantCulture);
            CacheDirectoryTextBox.Text = _cacheDirectory;
            PlaybackFollowsCursorCheckBox.IsChecked = _playbackFollowsCursor;
            AlbumArtSizeTextBox.Text = _albumArtSize.ToString(CultureInfo.InvariantCulture);
            SavePlaybackListCheckBox.IsChecked = _savePlaybackList;
            SaveWorkingPlaylistCheckBox.IsChecked = _saveWorkingPlaylist;
            ShowAlbumArtCheckBox.IsChecked = _showAlbumArt;
            AlbumDataGridAlbumArtColumn.Visibility = !_showAlbumArt ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            SocalAlbumArtColumn.Visibility = !_showAlbumArt ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            AlbumDataGridEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
            SocialEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
            DoubleClickComboBox.SelectedItem = _doubleClickBehavior;
            CachePlaylistTracksCheckBox.IsChecked = _cachePlaylistTracks;
            AlbumPlayButtonBehaviorComboBox.SelectedItem = _albumPlayButtonBehavior;

            SetProxyEntryVisibility(UseProxy);
            SetAlbumArtSizeVisibility(_showAlbumArt);
        }

        private void PopulatePlaylist()
        {
            Dispatcher.Invoke(() =>
                                  {
                                      if (!string.IsNullOrWhiteSpace(_currentPlaylist))
                                      {
                                          ObservableCollection<TrackItem> playlistTrackItems;
                                          var xmlSerializer = new XmlSerializer(_playlistTrackItems.GetType());

                                          using (TextReader reader = new StringReader(_currentPlaylist))
                                              playlistTrackItems = xmlSerializer.Deserialize(reader) as ObservableCollection<TrackItem>;

                                          if (playlistTrackItems != null)
                                              foreach (TrackItem trackItem in playlistTrackItems)
                                                  AddTrackItemToPlaylist(trackItem, false, false);
                                      }
                                  });
        }

        private void PopulatePlaybackList()
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(_currentPlaybackList))
                {
                    ObservableCollection<TrackItem> playbackTrackItems;
                    var xmlSerializer = new XmlSerializer(_playbackTrackItems.GetType());

                    using (TextReader reader = new StringReader(_currentPlaybackList))
                        playbackTrackItems = xmlSerializer.Deserialize(reader) as ObservableCollection<TrackItem>;

                    if (playbackTrackItems != null)
                        foreach (TrackItem trackItem in playbackTrackItems)
                            AddTrackItemToPlaylist(trackItem, true, false);
                }
            });
        }

        private void PopulateMaxBitrateComboBox()
        {
            var listData = new List<int> { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 };

            MaxBitrateComboBox.ItemsSource = listData;
            MaxBitrateComboBox.SelectedItem = _maxBitrate;
        }

        private void PopulateDoubleClickComboBox()
        {
            var listData = new List<DoubleClickBehavior> { DoubleClickBehavior.Add, DoubleClickBehavior.Play };

            DoubleClickComboBox.ItemsSource = listData;
            DoubleClickComboBox.SelectedItem = DoubleClickBehavior.Add;
        }

        private void PopulateAlbumPlayBehaviorComboBox()
        {
            var listData = new List<AlbumPlayButtonBehavior> { AlbumPlayButtonBehavior.Ask, AlbumPlayButtonBehavior.Play };

            AlbumPlayButtonBehaviorComboBox.ItemsSource = listData;
            AlbumPlayButtonBehaviorComboBox.SelectedItem = AlbumPlayButtonBehavior.Ask;
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

        private void SetAlbumArtSizeVisibility(bool isChecked)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      ThrottleTextBox.IsEnabled = isChecked;
                                      AlbumArtSizeTextBox.IsEnabled = isChecked;
                                  });
        }
    }
}
