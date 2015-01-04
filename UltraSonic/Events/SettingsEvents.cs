using Subsonic.Common.Classes;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UltraSonic.Items;
using UltraSonic.Properties;
using UltraSonic.Static;
using Directory = System.IO.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Filter out non-digit text input
            foreach (char c in e.Text.Where(c => !Char.IsDigit(c)))
            {
                e.Handled = true;
                break;
            }
        }

        private void SettingsSaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Username = SettingsUsernameTextBox.Text;
                Password = SettingsPasswordPasswordBox.Password;
                ServerUrl = SettingsServerAddressTextBox.Text;

                ProxyServer = SettingsProxyServerAddressTextBox.Text;
                ProxyUsername = SettingsProxyServerUsernameTextBox.Text;
                ProxyPassword = SettingsProxyServerPasswordTextBox.Password;
                bool? isChecked = SettingsUseProxyCheckbox.IsChecked;
                UseProxy = isChecked.HasValue && isChecked.Value;
                _playbackFollowsCursor = PlaybackFollowsCursorCheckBox.IsChecked.HasValue && PlaybackFollowsCursorCheckBox.IsChecked.Value;

                int? proxyPort = ValidateInt(1, 65535, SettingsProxyServerPortTextBox.Text);
                ProxyPort = proxyPort.HasValue ? proxyPort.Value : 0;
                int? maxSearchResults = ValidateInt(0, 0, MaxSearchResultsTextBox.Text);
                _maxSearchResults = maxSearchResults.HasValue ? maxSearchResults.Value : 25;
                int? throttle = ValidateInt(0, int.MaxValue, ThrottleTextBox.Text);
                _throttle = throttle.HasValue ? throttle.Value : 6;
                int? albumListMax = ValidateInt(1, int.MaxValue, AlbumListMaxTextBox.Text);
                _albumListMax = albumListMax.HasValue ? albumListMax.Value : 25;
                int? nowPlayingInterval = ValidateInt(0, int.MaxValue, NowPlayingIntervalTextBox.Text);
                _nowPlayingInterval = nowPlayingInterval.HasValue ? nowPlayingInterval.Value : 30;
                int? chatMessagesInterval = ValidateInt(0, int.MaxValue, ChatMessagesIntervalTextBox.Text);
                _chatMessagesInterval = chatMessagesInterval.HasValue ? chatMessagesInterval.Value : 5;
                int? albumArtSize = ValidateInt(1, int.MaxValue, AlbumArtSizeTextBox.Text);
                _albumArtSize = albumArtSize.HasValue ? albumArtSize.Value : 50;

                _streamParameters = new Subsonic.Common.StreamParameters
                {
                    BitRate = (int) MaxBitrateComboBox.SelectedValue
                };

                _cacheDirectory = CacheDirectoryTextBox.Text;
                _serverHash = StaticMethods.CalculateSha256(ServerUrl, Encoding.Unicode);
                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _musicCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "Music");
                _coverArtCacheDirectoryName = Path.Combine(Path.Combine(_cacheDirectory, _serverHash), "CoverArt");
                _saveWorkingPlaylist = SaveWorkingPlaylistCheckBox.IsChecked.HasValue && SaveWorkingPlaylistCheckBox.IsChecked.Value;
                _savePlaybackList = SavePlaybackListCheckBox.IsChecked.HasValue && SavePlaybackListCheckBox.IsChecked.Value;
                _showAlbumArt = ShowAlbumArtCheckBox.IsChecked.HasValue && ShowAlbumArtCheckBox.IsChecked.Value;
                _doubleClickBehavior = (DoubleClickBehavior) DoubleClickComboBox.SelectedValue;
                _albumPlayButtonBehavior = (AlbumPlayButtonBehavior) AlbumPlayButtonBehaviorComboBox.SelectedValue;
                _cachePlaylistTracks = CachePlaylistTracksCheckBox.IsChecked.HasValue && CachePlaylistTracksCheckBox.IsChecked.Value;

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

                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);
                _chatMessagesTimer.Interval = TimeSpan.FromSeconds(_chatMessagesInterval);

                Settings.Default.Username = Username;
                Settings.Default.Password = Password;
                Settings.Default.ServerUrl = ServerUrl;
                Settings.Default.UseProxy = UseProxy;
                Settings.Default.ProxyServer = ProxyServer;
                Settings.Default.ProxyPort = ProxyPort;
                Settings.Default.ProxyUsername = ProxyUsername;
                Settings.Default.ProxyPassword = ProxyPassword;
                Settings.Default.MaxSearchResults = _maxSearchResults;
                Settings.Default.MaxBitrate = _streamParameters.BitRate;
                Settings.Default.AlbumListMax = _albumListMax;
                Settings.Default.CacheDirectory = _cacheDirectory;
                Settings.Default.NowPlayingInterval = _nowPlayingInterval;
                Settings.Default.ChatMessagesInterval = _chatMessagesInterval;
                Settings.Default.PlaybackFollowsCursor = _playbackFollowsCursor;
                Settings.Default.Throttle = _throttle;
                Settings.Default.AlbumArtSize = _albumArtSize;
                Settings.Default.SaveWorkingPlaylist = _saveWorkingPlaylist;
                Settings.Default.SavePlaybackList = _savePlaybackList;
                Settings.Default.ShowAlbumArt = _showAlbumArt;
                Settings.Default.DoubleClickBehavior = Enum.GetName(typeof (DoubleClickBehavior), _doubleClickBehavior);
                Settings.Default.AlbumPlayButtonBehavior = Enum.GetName(typeof (AlbumPlayButtonBehavior), _albumPlayButtonBehavior);
                Settings.Default.CachePlaylistTracks = _cachePlaylistTracks;

                Settings.Default.Save();

                InitSubsonicApi();

                if (SubsonicClient == null)
                    return;

                ConfigureChat(_chatMessagesInterval);

                SubsonicClient.GetLicenseAsync().ContinueWith(CheckLicense);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception:\n\n{0}\n{1}", ex.Message, ex.StackTrace), AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckLicense(Task<License> task)
        {
            License license = task.Result;

            if (!license.Valid)
            {
                MessageBox.Show(string.Format("You must have a valid REST API license to use {0}", AppName));
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateLicenseInformation(license);
                    UpdateArtists();
                    UpdatePlaylists();
                    UpdateNowPlaying();
                    //UpdateChatMessages();
                    MusicTab.Focus();
                });
            }
        }

        private static int? ValidateInt(int min, int max, string value)
        {
            int intValue;
            if (!int.TryParse(value, out intValue)) return null;
            if (min == 0 && max == 0) return intValue;
            if (min <= intValue && intValue <= max) return intValue;
            return null;
        }

        private void SettingsUseProxyCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetProxyEntryVisibility(true);
        }

        private void ShowAlbumArtCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetAlbumArtSizeVisibility(true);
        }

        private void ShowAlbumArtCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetAlbumArtSizeVisibility(false);
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

                                      foreach (UltraSonicAlbumItem albumItem in AlbumDataGrid.Items)
                                      {
                                        if (albumItem.Image != null) albumDataGridArtAvailable = true;
                                          break;
                                      }

                                      foreach (UltraSonicNowPlayingItem nowPlayingItem in NowPlayingDataGrid.Items)
                                      {
                                        if (nowPlayingItem.Image != null) nowPlayingDataGridArtAvailable = true;
                                          break;
                                      }

                                      if (_showAlbumArt && albumDataGridArtAvailable)
                                          AlbumDataGridAlbumArtColumn.Visibility = Visibility.Visible;
                                      else
                                          AlbumDataGridAlbumArtColumn.Visibility = Visibility.Collapsed;

                                      if (_showAlbumArt && nowPlayingDataGridArtAvailable)
                                          SocalAlbumArtColumn.Visibility = Visibility.Visible;
                                      else
                                          SocalAlbumArtColumn.Visibility = Visibility.Collapsed;

                                      ShowAlbumArtCheckBox.IsChecked = _showAlbumArt;

                                      AlbumDataGridEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
                                      SocialEnableCoverArt.Header = _showAlbumArt ? "Disable Cover Art" : "Enable Cover Art";
                                  });
        }
    }
}
