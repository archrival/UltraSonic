using System.Globalization;
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
                _cacheDirectory = CacheDirectoryTextBox.Text;
                _serverHash = StaticMethods.CalculateSha256(ServerUrl, Encoding.Unicode);
                _useDiskCache = UseDiskCacheCheckBox.IsChecked.HasValue && UseDiskCacheCheckBox.IsChecked.Value;
                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
                _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");
                _saveWorkingPlaylist = SaveWorkingPlaylistCheckBox.IsChecked.HasValue && SaveWorkingPlaylistCheckBox.IsChecked.Value;
                _showAlbumArt = ShowAlbumArtCheckBox.IsChecked.HasValue && ShowAlbumArtCheckBox.IsChecked.Value;
                _doubleClickBehavior = (DoubleClickBehavior) DoubleClickComboBox.SelectedValue;

                if (!int.TryParse(AlbumArtSizeTextBox.Text, out _albumArtSize))
                {
                    _albumArtSize = 50;
                    AlbumArtSizeTextBox.Text = _albumArtSize.ToString(CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrWhiteSpace(ServerUrl))
                {
                    if (!Directory.Exists(_musicCacheDirectoryName))
                        Directory.CreateDirectory(_musicCacheDirectoryName);

                    if (!Directory.Exists(_coverArtCacheDirectoryName))
                        Directory.CreateDirectory(_coverArtCacheDirectoryName);
                }

                AlbumDataGridAlbumArtColumn.Visibility = !_showAlbumArt ? Visibility.Collapsed : Visibility.Visible;
                SocalAlbumArtColumn.Visibility = !_showAlbumArt ? Visibility.Collapsed : Visibility.Visible;
                AlbumDataGridEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
                SocialEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";

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
                Settings.Default.PlaybackFollowsCursor = _playbackFollowsCursor;
                Settings.Default.Throttle = _throttle;
                Settings.Default.AlbumArtSize = _albumArtSize;
                Settings.Default.SaveWorkingPlaylist = _saveWorkingPlaylist;
                Settings.Default.ShowAlbumArt = _showAlbumArt;
                Settings.Default.DoubleClickBehavior = Enum.GetName(typeof (DoubleClickBehavior), _doubleClickBehavior);

                Settings.Default.Save();

                InitSubsonicApi();

                License license = SubsonicApi.GetLicense();

                if (!license.Valid)
                {
                    MessageBox.Show(string.Format("You must have a valid REST API license to use {0}", AppName));
                }
                else
                {
                    UpdateLicenseInformation(license);
                    UpdateArtists();
                    UpdatePlaylists();
                    UpdateNowPlaying();
                    UpdateChatMessages();
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

        private void ShowAlbumArtCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetAlbumArtSizeVisibility(true);
        }

        private void ShowAlbumArtCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetAlbumArtSizeVisibility(false);
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

        private void AlbumDataGridEnableCoverArtClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      _showAlbumArt = !_showAlbumArt;

                                      bool albumDataGridArtAvailable = false;
                                      bool nowPlayingDataGridArtAvailable = false;

                                      foreach (AlbumItem albumItem in AlbumDataGrid.Items)
                                      {
                                          if (albumItem.Image != null) albumDataGridArtAvailable = true;
                                          break;
                                      }

                                      foreach (NowPlayingItem nowPlayingItem in NowPlayingDataGrid.Items)
                                      {
                                          if (nowPlayingItem.Image != null) nowPlayingDataGridArtAvailable = true;
                                          break;
                                      }

                                      if (_showAlbumArt && albumDataGridArtAvailable)
                                      {
                                          AlbumDataGridAlbumArtColumn.Visibility = Visibility.Visible;
                                          AlbumDataGrid.Items.Refresh();
                                      }
                                      else
                                      {
                                          AlbumDataGridAlbumArtColumn.Visibility = Visibility.Collapsed;
                                      }

                                      if (_showAlbumArt && nowPlayingDataGridArtAvailable)
                                      {
                                          SocalAlbumArtColumn.Visibility = Visibility.Visible;
                                          NowPlayingDataGrid.Items.Refresh();
                                      }
                                      else
                                      {
                                          SocalAlbumArtColumn.Visibility = Visibility.Collapsed;
                                      }

                                      ShowAlbumArtCheckBox.IsChecked = _showAlbumArt;

                                      AlbumDataGridEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
                                      SocialEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
                                  });
        }
    }
}
