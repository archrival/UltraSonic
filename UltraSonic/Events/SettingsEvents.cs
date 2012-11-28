using Subsonic.Rest.Api;
using System;
using System.IO;
using System.Text;
using System.Windows;
using UltraSonic.Properties;
using UltraSonic.Static;
using Directory = System.IO.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void SettingsSaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Username = SettingsUsernameTextBox.Text;
                Password = SettingsPasswordPasswordBox.Password;
                ServerUrl = SettingsServerAddressTextBox.Text;

                int proxyPort;

                ProxyServer = SettingsProxyServerAddressTextBox.Text;
                int.TryParse(SettingsProxyServerPortTextBox.Text, out proxyPort);

                ProxyPort = proxyPort;
                ProxyUsername = SettingsProxyServerUsernameTextBox.Text;
                ProxyPassword = SettingsProxyServerPasswordTextBox.Password;
                bool? isChecked = SettingsUseProxyCheckbox.IsChecked;
                UseProxy = isChecked.HasValue && isChecked.Value;
                _playbackFollowsCursor = PlaybackFollowsCursorCheckBox.IsChecked.HasValue && PlaybackFollowsCursorCheckBox.IsChecked.Value;
                _maxSearchResults = (int)MaxSearchResultsComboBox.SelectedValue;
                _maxBitrate = (int)MaxBitrateComboBox.SelectedValue;
                _throttle = (int)ThrottleComboBox.SelectedValue;
                _albumListMax = (int)AlbumListMaxComboBox.SelectedValue;
                _nowPlayingInterval = (int)NowPlayingIntervalComboBox.SelectedValue;
                _chatMessagesInterval = (int)ChatMessagesIntervalComboBox.SelectedValue;
                _cacheDownloadLimit = (int)CacheDownloadLimitComboBox.SelectedValue;
                _cacheDirectory = CacheDirectoryTextBox.Text;
                _serverHash = StaticMethods.CalculateSha256(ServerUrl, Encoding.Unicode);
                _useDiskCache = UseDiskCacheCheckBox.IsChecked.HasValue && UseDiskCacheCheckBox.IsChecked.Value;
                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
                _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");
                _saveWorkingPlaylist = SaveWorkingPlaylistCheckBox.IsChecked.HasValue && SaveWorkingPlaylistCheckBox.IsChecked.Value;
                if (!int.TryParse(AlbumArtSizeTextBox.Text, out _albumArtSize))
                {
                    _albumArtSize = 150;
                    AlbumArtSizeTextBox.Text = _albumArtSize.ToString();
                }
                

                if (!string.IsNullOrWhiteSpace(ServerUrl))
                {
                    if (!Directory.Exists(_musicCacheDirectoryName))
                        Directory.CreateDirectory(_musicCacheDirectoryName);

                    if (!Directory.Exists(_coverArtCacheDirectoryName))
                        Directory.CreateDirectory(_coverArtCacheDirectoryName);
                }

                Settings.Default.Username = Username;
                Settings.Default.Password = Password;
                Settings.Default.ServerUrl = ServerUrl;
                Settings.Default.UseProxy = UseProxy;
                Settings.Default.ProxyServer = ProxyServer;
                Settings.Default.ProxyPort = ProxyPort;
                Settings.Default.ProxyUsername = ProxyUsername;
                Settings.Default.ProxyPassword = ProxyPassword;
                Settings.Default.MaxSearchResults = _maxSearchResults;
                Settings.Default.MaxBitrate = _maxBitrate;
                Settings.Default.AlbumListMax = _albumListMax;
                Settings.Default.CacheDirectory = _cacheDirectory;
                Settings.Default.UseDiskCache = _useDiskCache;
                Settings.Default.NowPlayingInterval = _nowPlayingInterval;
                Settings.Default.ChatMessagesInterval = _chatMessagesInterval;
                Settings.Default.CacheDownloadLimit = _cacheDownloadLimit;
                Settings.Default.PlaybackFollowsCursor = _playbackFollowsCursor;
                Settings.Default.Throttle = _throttle;
                Settings.Default.AlbumArtSize = _albumArtSize;
                Settings.Default.SaveWorkingPlaylist = _saveWorkingPlaylist;

                Settings.Default.Save();

                InitSubsonicApi();

                License license = SubsonicApi.GetLicense();

                if (!license.Valid)
                {
                    MessageBox.Show(string.Format("You must have a valid REST API license to use {0}", AppName));
                }
                else
                {
                    UpdateArtists();
                    UpdatePlaylists();
                    MusicTab.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n{1}", ex.Message, ex.StackTrace), string.Format("Exception in {0}", AppName), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SettingsUseProxyCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetProxyEntryVisibility(true);
        }

        private void SettingsUseDiskCacheCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetUseDiskCacheVisibility(false);
        }

        private void SettingsUseDiskCacheCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetUseDiskCacheVisibility(true);
        }

        private void SettingsCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MusicTab.Focus();
                PopulateSettings();
            });
        }

        private void SettingsUseProxyCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetProxyEntryVisibility(false);
        }
    }
}
