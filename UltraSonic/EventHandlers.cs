using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Subsonic.Rest.Api;
using Subsonic.Rest.Api.Enums;
using UltraSonic.Properties;
using CheckBox = System.Windows.Controls.CheckBox;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PreferencesSaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Username = PreferencesUsernameTextBox.Text;
                Password = PreferencesPasswordPasswordBox.Password;
                ServerUrl = PreferencesServerAddressTextBox.Text;

                int proxyPort;

                ProxyServer = PreferencesProxyServerAddressTextBox.Text;
                int.TryParse(PreferencesProxyServerPortTextBox.Text, out proxyPort);

                ProxyPort = proxyPort;
                ProxyUsername = PreferencesProxyServerUsernameTextBox.Text;
                ProxyPassword = PreferencesProxyServerPasswordTextBox.Password;
                bool? isChecked = PreferencesUseProxyCheckbox.IsChecked;
                UseProxy = isChecked.HasValue && isChecked.Value;
                _maxSearchResults = (int) MaxSearchResultsComboBox.SelectedValue;
                _maxBitrate = (int) MaxBitrateComboBox.SelectedValue;
                _albumListMax = (int) AlbumListMaxComboBox.SelectedValue;
                _nowPlayingInterval = (int) NowPlayingIntervalComboBox.SelectedValue;
                _chatMessagesInterval = (int) ChatMessagesIntervalComboBox.SelectedValue;
                _cacheDirectory = CacheDirectoryTextBox.Text;
                if (UseDiskCacheCheckBox.IsChecked != null) _useDiskCache = UseDiskCacheCheckBox.IsChecked.Value;

                _nowPlayingTimer.Interval = TimeSpan.FromSeconds(_nowPlayingInterval);

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

        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playlistTrackItems.Any();
        }

        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playlistTrackItems.Any() && PlaylistTrackGrid.SelectedIndex > 0;
        }

        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayer != null && _playlistTrackItems.Any() && PlaylistTrackGrid.SelectedIndex < PlaylistTrackGrid.Items.Count - 1;
        }

        private void ShuffleButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.Invoke(() => _playlistTrackItems.Shuffle());
        }

        private void MuteButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                    VolumeSlider.IsEnabled = !MediaPlayer.IsMuted;
                    SetVolumeMetadata();
                });
        }

        private void SetVolumeMetadata()
        {
            VolumeSlider.ToolTip = MediaPlayer.IsMuted ? "Volume: Muted" : string.Format("Volume: {0}%", Math.Round(MediaPlayer.Volume*100, 0));
            MuteButton.ToolTip = MediaPlayer.IsMuted ? "UnMute Volume" : "Mute Volume";

            if (MediaPlayer.IsMuted)
                MuteButtonIcon.Name = "VolumeMuted";
            else if (MediaPlayer.Volume >= 0 && MediaPlayer.Volume <= (double) 1/(double) 4)
                MuteButtonIcon.Name = "VolumeZero";
            else if (MediaPlayer.Volume > (double) 1/(double) 4 && MediaPlayer.Volume <= (double) 1/(double) 2)
                MuteButtonIcon.Name = "VolumeLow";
            else if (MediaPlayer.Volume > (double) 1/(double) 2 && MediaPlayer.Volume <= (double) 3/(double) 4)
                MuteButtonIcon.Name = "VolumeMedium";
            else if (MediaPlayer.Volume > (double) 3/(double) 4 && MediaPlayer.Volume <= 1)
                MuteButtonIcon.Name = "VolumeHigh";
        }

        private void PlayButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    if (MediaPlayer.Source != null)
                    {
                        PlayMusic();
                    }
                    else
                    {
                        if (PlaylistTrackGrid.Items.Count > 0)
                        {
                            if (PlaylistTrackGrid.SelectedIndex == -1)
                                PlaylistTrackGrid.SelectedIndex = 0;

                            var playlistEntryItem = PlaylistTrackGrid.SelectedItem as TrackItem;

                            if (playlistEntryItem != null)
                                QueueTrack(playlistEntryItem.Track);
                        }
                    }
                });
        }

        private void UpdateTitle()
        {
            string title = AppName;

            if (MediaPlayer.Source != null)
            {
                title = string.Format("{0} - {1} - {2} [{3}]", AppName, _currentArtist, _currentTitle, MusicPlayStatusLabel.Content);
                MusicArtistLabel.Content = _currentArtist;
                MusicTitleLabel.Content = _currentTitle;
                MusicAlbumLabel.Content = _currentAlbum;
            }

            Title = title;
        }

        private void PlayMusic()
        {
            if (MediaPlayer.Source != null)
                MediaPlayer.Play();

            MusicPlayStatusLabel.Content = "Playing";
        }

        private void PauseMusic()
        {
            if (MediaPlayer.Source != null)
                MediaPlayer.Pause();

            MusicPlayStatusLabel.Content = "Paused";
        }

        private void StopMusic()
        {
            if (MediaPlayer.Source != null)
            {
                MediaPlayer.Stop();
                MediaPlayer.Source = null;
            }
            MusicPlayStatusLabel.Content = "Stopped";
        }

        private void PauseButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    if (MediaPlayer.Source != null && MediaPlayer.CanPause)
                    {
                        if ((string) MusicPlayStatusLabel.Content == "Paused")
                            PlayMusic();
                        else
                            PauseMusic();
                    }
                });
        }

        private void StopButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    if (MediaPlayer.Source != null)
                        StopMusic();
                });
        }

        private void PlayNextTrack()
        {
            Dispatcher.Invoke(() =>
                {
                    MediaPlayer.Stop();

                    bool playNextTrack = false;

                    if (PlaylistTrackGrid.SelectedIndex == PlaylistTrackGrid.Items.Count - 1)
                    {
                        if (_repeatPlaylist)
                        {
                            PlaylistTrackGrid.SelectedIndex = 0;
                            playNextTrack = true;
                        }
                    }
                    else
                    {
                        PlaylistTrackGrid.SelectedIndex++;
                        playNextTrack = true;
                    }

                    if (playNextTrack)
                    {
                        TrackItem trackItem = PlaylistTrackGrid.SelectedItem as TrackItem;

                        if (trackItem != null)
                            PlayTrack(trackItem.Track);
                    }
                    else
                    {
                        StopMusic();
                    }
                });
        }

        private void PlayPreviousTrack()
        {
            Dispatcher.Invoke(() =>
                {
                    MediaPlayer.Stop();
                    PlaylistTrackGrid.SelectedIndex--;

                    TrackItem trackItem = PlaylistTrackGrid.SelectedItem as TrackItem;

                    if (trackItem != null)
                        PlayTrack(trackItem.Track);
                });
        }

        private void NextButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(PlayNextTrack);
        }

        private void PreviousButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(PlayPreviousTrack);
        }

        private void MediaPlayerMediaOpened(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    _position = MediaPlayer.NaturalDuration.TimeSpan;
                    ProgressSlider.Minimum = 0;
                    ProgressSlider.Maximum = _position.TotalMilliseconds;
                });
        }

        private void MusicTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var dataItem = e.NewValue as ArtistItem;

            if (dataItem != null && dataItem.Artist != null)
                Dispatcher.Invoke(() => { SubsonicApi.GetMusicDirectoryAsync(dataItem.Artist.Id, GetCancellationToken("MusicTreeViewSelectionItemChanged")).ContinueWith(UpdateAlbumGrid); });
        }

        private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(() =>
                {
                    MediaPlayer.Volume = e.NewValue/10;
                    SetVolumeMetadata();
                });
        }

        private void ProgressSliderMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int sliderValue = (int) ProgressSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            MediaPlayer.Position = ts;
        }

        private void MusicDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            AlbumItem albumItem = MusicDataGrid.SelectedItem as AlbumItem;

            if (albumItem != null)
            {
                Dispatcher.Invoke(() =>
                    {
                        SubsonicApi.GetMusicDirectoryAsync(albumItem.Album.Id, GetCancellationToken("MusicDataGridSelectionChanged")).ContinueWith(UpdateTrackListingGrid);
                    });
            }
        }

        private void SavePlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            bool updatePlaylist = false;

            if (CurrentPlaylist != null)
            {
                MessageBoxResult result = MessageBox.Show(string.Format("Would you like to update the previously loaded playlist? '{0}'", CurrentPlaylist.Name), "Save playlist", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                updatePlaylist = (result == MessageBoxResult.Yes);
            }

            if (updatePlaylist)
            {
                Dispatcher.Invoke(() =>
                {
                        List<string> playlistTracks = new List<string>();
                        playlistTracks.AddRange(_playlistTrackItems.Select(test => test.Track.Id));

                        SubsonicApi.CreatePlaylistAsync(CurrentPlaylist.Id, null, playlistTracks).ContinueWith(CheckPlaylistSave);
                });
            }
            else
            {
                string playlistName = null;
                SavePlaylist savePlaylist = new SavePlaylist {SavePlaylistLabel = {Content = "Please enter a name for the playlist."}, Owner = this};
                savePlaylist.ShowDialog();

                if (savePlaylist.PlaylistName != null)
                    playlistName = savePlaylist.PlaylistName;

                Dispatcher.Invoke(() =>
                                      {
                                              List<string> playlistTracks = new List<string>();
                                              playlistTracks.AddRange(_playlistTrackItems.Select(test => test.Track.Id));

                                              SubsonicApi.CreatePlaylistAsync(null, playlistName, playlistTracks).ContinueWith(CheckPlaylistSave);
                                      });
            }
        }

        private void NewPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to clear the current playlist?", "New playlist", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                Dispatcher.Invoke(() =>
                    {
                        CurrentPlaylist = null;
                        _playlistTrackItems.Clear();
                        PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                    });
        }

        private void PlaylistsDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            PlaylistItem playlistItem = PlaylistsDataGrid.SelectedItem as PlaylistItem;

            if (playlistItem != null)
            {
                if (playlistItem.Playlist == null && playlistItem.Name == "Starred")
                {
                    Dispatcher.Invoke(() =>
                        {
                            SubsonicApi.GetStarredAsync(GetCancellationToken("PlaylistsDataGridSelectionChanged")).ContinueWith(UpdatePlaylistGrid);
                        });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                        {
                            CurrentPlaylist = playlistItem.Playlist;
                            SubsonicApi.GetPlaylistAsync(playlistItem.Playlist.Id, GetCancellationToken("PlaylistsDataGridSelectionChanged")).ContinueWith(UpdatePlaylistGrid);
                        });
                }
            }
        }

        private void TrackDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid source = e.Source as DataGrid;

            if (source != null)
            {
                TrackItem selectedTrack = source.CurrentItem as TrackItem;

                if (selectedTrack != null)
                {
                    Dispatcher.Invoke(() =>
                        {
                            _playlistTrackItems.Add(selectedTrack);
                            PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                        });
                }
            }
        }

        private void MusicDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid source = e.Source as DataGrid;

            if (source != null)
            {
                AlbumItem selectedAlbum = source.CurrentItem as AlbumItem;

                if (selectedAlbum != null)
                {
                    Dispatcher.Invoke(() =>
                        {
                            SubsonicApi.GetMusicDirectoryAsync(selectedAlbum.Album.Id, GetCancellationToken("MusicDataGridMouseDoubleClick")).ContinueWith(AddAlbumToPlaylist);
                        });
                }
            }
        }

        private void PreferencesUseProxyCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetProxyEntryVisibility(false);
        }

        private void PreferencesUseProxyCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetProxyEntryVisibility(true);
        }

        private void UseDiskCacheCheckboxUnChecked(object sender, RoutedEventArgs e)
        {
            SetUseDiskCacheVisibility(false);
        }

        private void UseDiskCacheCheckboxChecked(object sender, RoutedEventArgs e)
        {
            SetUseDiskCacheVisibility(true);
        }

        private void RepeatButtonClick(object sender, RoutedEventArgs e)
        {
            _repeatPlaylist = !_repeatPlaylist;

            Dispatcher.Invoke(() =>
                {
                    RepeatButtonIcon.Name = _repeatPlaylist ? "RepeatEnabled" : "RepeatDisabled";
                    RepeatButton.ToolTip = _repeatPlaylist ? "Repeat: Enabled" : "Repeat: Disabled";
                });
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    Settings.Default.Volume = MediaPlayer.Volume;
                    Settings.Default.Height = Height;
                    Settings.Default.Width = Width;
                    Settings.Default.Save();
                });
        }

        private void ArtistFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var test = e.Source as TextBox;

            Dispatcher.Invoke(() =>
                {
                    if (test != null) _artistFilter = test.Text;
                    MusicTreeView.ItemsSource = ArtistItems;
                });
        }

        private void GlobalSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string searchQuery = GlobalSearchTextBox.Text;

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    MusicDataGrid.ItemsSource = null;
                    TrackDataGrid.ItemsSource = null;
                    SearchStatusLabel.Content = "Searching...";
                    SubsonicApi.Search2Async(searchQuery, _maxSearchResults, 0, _maxSearchResults, 0, _maxSearchResults, 0, GetCancellationToken("GlobalSearchTextBoxKeyDown")).ContinueWith(PopulateSearchResults);
                }
            }
        }

        private void ChatListTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string chatMessage = ChatListInput.Text;

                if (!string.IsNullOrWhiteSpace(chatMessage))
                {
                    SubsonicApi.AddChatMessageAsync(chatMessage).ContinueWith(t => UpdateChatMessages());
                    ChatListInput.Text = string.Empty;
                }
            }
        }

        private void TrackDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => DownloadTracks(TrackDataGrid.SelectedItems));
        }


        private void DownloadTracks(ICollection selectedItems)
        {
            foreach (TrackItem item in selectedItems)
            {
                Process.Start(SubsonicApi.BuildDownloadUrl(item.Track.Id));
            }
        }

        private void PlaylistTrackGridDownloadClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => DownloadTracks(PlaylistTrackGrid.SelectedItems));
        }

        private void MusicDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = MusicDataGrid.SelectedItems;

            foreach (AlbumItem item in selectedItems)
            {
                Process.Start(SubsonicApi.BuildDownloadUrl(item.Album.Id));
            }
        }

        private void MusicDataGridAlbumListClick(object sender, RoutedEventArgs e)
        {
            MenuItem source = e.Source as MenuItem;

            if (source != null)
            {
                AlbumListType albumListType;

                switch (source.Header.ToString())
                {
                    case "Newest":
                        albumListType = AlbumListType.newest;
                        break;
                    case "Random":
                        albumListType = AlbumListType.random;
                        break;
                    case "Highest Rated":
                        albumListType = AlbumListType.highest;
                        break;
                    case "Frequently Played":
                        albumListType = AlbumListType.frequent;
                        break;
                    case "Recently Played":
                        albumListType = AlbumListType.recent;
                        break;
                    case "Starred":
                        albumListType = AlbumListType.starred;
                        break;
                    default:
                        albumListType = AlbumListType.newest;
                        break;
                }

                Dispatcher.Invoke(() =>
                    {
                        SubsonicApi.GetAlbumListAsync(albumListType, _albumListMax, null, GetCancellationToken("MusicDataGridAlbumListClick")).ContinueWith(UpdateAlbumGrid);
                    });
            }
        }

        private void PlaylistsDataGridRefreshClick(object sender, RoutedEventArgs e)
        {
            UpdatePlaylists();
        }

        private void PlaylistsDataGridDeletePlaylistClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = PlaylistsDataGrid.SelectedItems;

            foreach (PlaylistItem item in selectedItems)
            {
                if (item.Playlist == null && item.Name == "Starred")
                {
                    MessageBox.Show("Playlist 'Starred' is a dynamic playlist and cannot be deleted.", "Delete playlist", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(string.Format("Would you like to delete the selected playlist? '{0}'", item.Name), "Delete playlist", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                        if (item.Playlist != null) SubsonicApi.DeletePlaylistAsync(item.Playlist.Id).ContinueWith(t => UpdatePlaylists());
                }
            }
        }

        void DataGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(CultureInfo.InvariantCulture);
        }

        private void MusicDataGridAddClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = MusicDataGrid.SelectedItems;

            Dispatcher.Invoke(() =>
                {
                    foreach (AlbumItem item in selectedItems)
                        SubsonicApi.GetMusicDirectoryAsync(item.Album.Id, GetCancellationToken("MusicDataGridAddClick")).ContinueWith(AddAlbumToPlaylist);
                });

        }

        private void TrackDataGridAddClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrackDataGrid.SelectedItems;

            Dispatcher.Invoke(() =>
                {
                    foreach (TrackItem item in selectedItems)
                        _playlistTrackItems.Add(item);

                    PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                });
        }

        private void PreferencesCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    MusicTab.Focus();
                    PopulateSettings();
                });
        }

        private void StarredCheckBoxClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    var source = e.Source as CheckBox;
                    var item = TrackDataGrid.CurrentItem as TrackItem;

                    if (item != null && source != null)
                    {
                        if (source.IsChecked.HasValue && source.IsChecked.Value)
                            SubsonicApi.StarAsync(new List<string> {item.Track.Id});
                        else
                            SubsonicApi.UnStarAsync(new List<string> {item.Track.Id});
                    }
                });
        }

        private void MusicDataGridStarredCheckBoxClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var source = e.Source as CheckBox;
                var item = MusicDataGrid.CurrentItem as AlbumItem;

                if (item != null && source != null)
                {
                    if (source.IsChecked.HasValue && source.IsChecked.Value)
                        SubsonicApi.StarAsync(new List<string> { item.Album.Id });
                    else
                        SubsonicApi.UnStarAsync(new List<string> { item.Album.Id });
                }
            });
        }

        private void NowPlayingGridStarredCheckBoxClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var source = e.Source as CheckBox;
                var item = NowPlayingDataGrid.CurrentItem as NowPlayingItem;

                if (item != null && source != null)
                {
                    if (source.IsChecked.HasValue && source.IsChecked.Value)
                        SubsonicApi.StarAsync(new List<string> { item.Entry.Id });
                    else
                        SubsonicApi.UnStarAsync(new List<string> { item.Entry.Id });
                }
            });
        }

        private void PlaylistStarredCheckBoxClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                {
                    var source = e.Source as CheckBox;
                    var item = PlaylistTrackGrid.CurrentItem as TrackItem;

                    if (item != null && source != null)
                    {
                        if (source.IsChecked.HasValue && source.IsChecked.Value)
                            SubsonicApi.StarAsync(new List<string> {item.Track.Id});
                        else
                            SubsonicApi.UnStarAsync(new List<string> {item.Track.Id});
                    }
                });
        }

        private void PlaylistTrackGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Dispatcher.Invoke(() =>
                {
                    StopMusic();

                    var playlistEntryItem = PlaylistTrackGrid.SelectedItem as TrackItem;

                    if (playlistEntryItem != null)
                        QueueTrack(playlistEntryItem.Track);
                });
        }

        private void PlaylistTrackGridUnloadingRow(object sender, DataGridRowEventArgs dataGridRowEventArgs)
        {
            Dispatcher.Invoke(() => PlaylistTrackGrid.Items.Refresh());
        }

        private void ArtistRefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateArtists();
        }
    }
}