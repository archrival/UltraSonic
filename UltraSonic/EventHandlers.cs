using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Subsonic.Rest.Api;
using UltraSonic.Properties;
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
                _maxSearchResults = (int)MaxSearchResultsComboBox.SelectedValue;

                Settings.Default.Username = Username;
                Settings.Default.Password = Password;
                Settings.Default.ServerUrl = ServerUrl;
                Settings.Default.UseProxy = UseProxy;
                Settings.Default.ProxyServer = ProxyServer;
                Settings.Default.ProxyPort = ProxyPort;
                Settings.Default.ProxyUsername = ProxyUsername;
                Settings.Default.ProxyPassword = ProxyPassword;
                Settings.Default.MaxSearchResults = _maxSearchResults;

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
                }
            }
            catch (Exception ex)
            {
                
            }

        }

        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ObservableCollection<TrackItem> playlist = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem>;
            e.CanExecute = MediaPlayer != null && playlist != null && playlist.Any();
        }

        private void PreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ObservableCollection<TrackItem> playlist = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem>;
            e.CanExecute = MediaPlayer != null && playlist != null && playlist.Any() && PlaylistTrackGrid.SelectedIndex > 0;
        }

        private void NextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ObservableCollection<TrackItem> playlist = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem>;
            e.CanExecute = MediaPlayer != null && playlist != null && playlist.Any() && PlaylistTrackGrid.SelectedIndex < PlaylistTrackGrid.Items.Count - 1;
        }

        private void ShuffleButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      ObservableCollection<TrackItem> playlist = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem>;

                                      if (playlist != null)
                                        playlist.Shuffle();
                                  });
        }

        private void MuteButtonClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
                                      VolumeSlider.IsEnabled = !MediaPlayer.IsMuted;
                                      SetVolumeIcon();                                         
                                  });
        }

        private void SetVolumeIcon()
        {
            VolumeSlider.ToolTip = MediaPlayer.IsMuted ? "Volume: Muted" : string.Format("Volume: {0}%", Math.Round(MediaPlayer.Volume*100, 0));

            if (MediaPlayer.IsMuted)
                MuteButtonIcon.Name = "VolumeMuted";
            else if (MediaPlayer.Volume >= 0 && MediaPlayer.Volume <= (double)1 / (double)4)
                MuteButtonIcon.Name = "VolumeZero";
            else if (MediaPlayer.Volume > (double)1 / (double)4 && MediaPlayer.Volume <= (double)1 / (double)2)
                MuteButtonIcon.Name = "VolumeLow";
            else if (MediaPlayer.Volume > (double)1 / (double)2 && MediaPlayer.Volume <= (double)3 / (double)4)
                MuteButtonIcon.Name = "VolumeMedium";
            else if (MediaPlayer.Volume > (double)3 / (double)4 && MediaPlayer.Volume <= 1)
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

                            MediaPlayer.MediaOpened += MediaPlayerPlayQueuedTrack;
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
                title = string.Format("{0} :: {1} : {2} [{3}]", AppName, _currentArtist, _currentTitle, MusicPlayStatusLabel.Content);

            Title = title;
        }

        private void PlayMusic()
        {
            MediaPlayer.Play();
            MusicPlayStatusLabel.Content = "Playing";
        }

        private void PauseMusic()
        {
            MediaPlayer.Pause();
            MusicPlayStatusLabel.Content = "Paused";
        }

        private void StopMusic()
        {
            MediaPlayer.Stop();
            MediaPlayer.Source = null;
            MusicPlayStatusLabel.Content = "Stopped";
        }

        private void MediaPlayerPlayQueuedTrack(object sender, RoutedEventArgs routedEventArgs)
        {
            MediaPlayer.MediaOpened -= MediaPlayerPlayQueuedTrack;
            PlayMusic();
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
                    ProgressSlider.Maximum = _position.TotalSeconds;
                });
        }

        private void MusicTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var dataItem = e.NewValue as ArtistItem;

            if (dataItem != null && dataItem.Artist != null)
                Dispatcher.Invoke(() => { SubsonicApi.GetMusicDirectoryAsync(dataItem.Artist.Id).ContinueWith(UpdateAlbumGrid); });
        }

        private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      MediaPlayer.Volume = e.NewValue/10;
                                      SetVolumeIcon();
                                  });
        }

        private void ProgressSliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int pos = Convert.ToInt32(ProgressSlider.Value);
            MediaPlayer.Position = new TimeSpan(0, 0, 0, pos, 0);
        }

        private void MusicDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlbumItem albumItem = null;

            if (e.AddedItems.Count > 0)
                albumItem = e.AddedItems[0] as AlbumItem;

            if (albumItem != null)
            {
                Dispatcher.Invoke(() =>
                {
                    SubsonicApi.GetMusicDirectoryAsync(albumItem.Album.Id).ContinueWith(UpdateTrackListingGrid);
                    //UpdateAlbumArt(albumItem.Album.Id);
                });
            }
        }

        private void SavePlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            string playlistName = null;
            SavePlaylist savePlaylist = new SavePlaylist {SavePlaylistLabel = {Content = "Please enter a name for the playlist."}, Owner = this};
            savePlaylist.ShowDialog();

            if (savePlaylist.PlaylistName != null)
                playlistName = savePlaylist.PlaylistName;

            Dispatcher.Invoke(() =>
                                  {
                                      ObservableCollection<TrackItem> observableCollection = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem>;

                                      if (observableCollection != null)
                                      {
                                          List<string> playlistTracks = new List<string>();
                                          playlistTracks.AddRange(observableCollection.Select(test => test.Track.Id));

                                          SubsonicApi.CreatePlaylistAsync(null, playlistName, playlistTracks).ContinueWith(CheckPlaylistSave);
                                      }
                                  });

        }

        private void NewPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to clear the current playlist?", "Clear Playlist", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                Dispatcher.Invoke(() =>
                                      {
                                          PlaylistTrackGrid.ItemsSource = new ObservableCollection<TrackItem>();
                                      });
        }

        private void PlaylistsDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlaylistItem playlistItem = null;

            if (e.AddedItems.Count > 0)
                playlistItem = e.AddedItems[0] as PlaylistItem;

            if (playlistItem != null)
            {
                Dispatcher.Invoke(() =>
                    {
                        SubsonicApi.GetPlaylistAsync(playlistItem.Playlist.Id).ContinueWith(UpdatePlaylistGrid);
                    });
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
                                              ObservableCollection<TrackItem> itemsSource = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem> ?? new ObservableCollection<TrackItem>();
                                              itemsSource.Add(selectedTrack);
                                              PlaylistTrackGrid.ItemsSource = itemsSource;
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
                                              SubsonicApi.GetMusicDirectoryAsync(selectedAlbum.Album.Id).ContinueWith(AddAlbumToPlaylist);
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

        private void RepeatButtonClick(object sender, RoutedEventArgs e)
        {
            _repeatPlaylist = !_repeatPlaylist;

            Dispatcher.Invoke(() =>
                {
                    RepeatButtonIcon.Name = _repeatPlaylist ? "RepeatEnabled" : "RepeatDisabled";
                    RepeatButton.ToolTip = _repeatPlaylist ? "Repeat Enabled" : "Repeat Disabled";
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
                    SubsonicApi.Search2Async(searchQuery, _maxSearchResults, 0, _maxSearchResults, 0, _maxSearchResults, 0).ContinueWith(PopulateSearchResults);
                }

            }
        }

        private void TrackGridDownloadClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      var selectedItems = TrackDataGrid.SelectedItems;

                                      if (selectedItems != null)
                                      {
                                          string downloadDirectory = FileDownloadDialog();

                                          foreach (TrackItem item in selectedItems)
                                              SubsonicApi.DownloadAsync(item.Track.Id, downloadDirectory, false);
                                      }
                                  });
        }

        private void MusicDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var selectedItems = MusicDataGrid.SelectedItems;

                if (selectedItems != null)
                {
                    string downloadDirectory = FileDownloadDialog();

                    foreach (AlbumItem item in selectedItems)
                        SubsonicApi.DownloadAsync(item.Album.Id, downloadDirectory, false);
                }
            });
        }
    }
}