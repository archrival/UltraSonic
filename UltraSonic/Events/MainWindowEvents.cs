using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using UltraSonic.Properties;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
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
                                                  QueueTrack(playlistEntryItem);
                                          }
                                      }
                                  });
        }

        private void PauseButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      if (MediaPlayer.Source == null || !MediaPlayer.CanPause) return;

                                      if ((string) MusicPlayStatusLabel.Content == "Paused")
                                          PlayMusic();
                                      else
                                          PauseMusic();
                                  });
        }

        private void StopButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(StopMusic);
        }

        private void NextButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(PlayNextTrack);
        }

        private void PreviousButtonClick(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(PlayPreviousTrack);
        }

        private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.Invoke(() =>
            {
                MediaPlayer.Volume = e.NewValue / 10;
                SetVolumeMetadata();
            });
        }

        private void ProgressSliderMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int sliderValue = (int)ProgressSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            MediaPlayer.Position = ts;
        }

        private void SavePlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null) return;

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
                    playlistTracks.AddRange(_playlistTrackItems.Select(test => test.Child.Id));

                    SubsonicApi.CreatePlaylistAsync(CurrentPlaylist.Id, null, playlistTracks).ContinueWith(CheckPlaylistSave);
                });
            }
            else
            {
                string playlistName = null;
                SavePlaylist savePlaylist = new SavePlaylist { SavePlaylistLabel = { Content = "Please enter a name for the playlist." }, Owner = this };
                savePlaylist.ShowDialog();

                if (savePlaylist.PlaylistName != null)
                    playlistName = savePlaylist.PlaylistName;

                Dispatcher.Invoke(() =>
                {
                    List<string> playlistTracks = new List<string>();
                    playlistTracks.AddRange(_playlistTrackItems.Select(test => test.Child.Id));

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
                });
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
            Settings.Default.Volume = MediaPlayer.Volume;
            Settings.Default.VolumeMuted = MediaPlayer.IsMuted;
            Settings.Default.WindowHeight = Height;
            Settings.Default.WindowWidth = Width;
            Settings.Default.WindowLeft = Left;
            Settings.Default.WindowTop = Top;
            Settings.Default.WindowMaximized = WindowState == WindowState.Maximized;

            if (_saveWorkingPlaylist)
            {
                string playlistXml;

                using (StringWriter textWriter = new StringWriter())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(_playlistTrackItems.GetType());
                    XmlWriter writer = XmlWriter.Create(textWriter);
                    xmlSerializer.Serialize(writer, _playlistTrackItems);
                    playlistXml = textWriter.ToString();
                }

                Settings.Default.CurrentPlaylist = playlistXml;
            }
            else
            {
                Settings.Default.CurrentPlaylist = string.Empty;
            }

            Settings.Default.Save();
        }

        private void GlobalSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string searchQuery = GlobalSearchTextBox.Text;

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    _albumItems.Clear();
                    _trackItems.Clear();
                    SearchStatusLabel.Content = "Searching...";
                    SubsonicApi.Search2Async(searchQuery, _maxSearchResults, 0, _maxSearchResults, 0, _maxSearchResults, 0, GetCancellationToken("GlobalSearchTextBoxKeyDown")).ContinueWith(PopulateSearchResults);
                }
            }
        }

        private void TrackInfoStackPanelMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlaylistTab.IsSelected = true;
            PlaylistTrackGrid.SelectedItem = _nowPlayingTrack;
        }

        private void MusicCoverArtMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentAlbumArt == null || AlbumArtWindow != null) return;

            BitmapSource bitmap;

            if (_currentAlbumArt.Height > ActualHeight * 0.9)
            {
                int newHeight = (int)(ActualHeight * 0.9);
                bitmap = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 0, newHeight);
            }
            else
            {
                bitmap = _currentAlbumArt.ToBitmapSource();
            }

            AlbumArt albumArtWindow = new AlbumArt
                                          {
                                              Height = bitmap.Height,
                                              Width = bitmap.Width,
                                              PopupAlbumArtImage = { Source = bitmap },
                                              Owner = this,
                                          };

            AlbumArtWindow = albumArtWindow;
            albumArtWindow.Show();
            Dwm.DropShadowToWindow(albumArtWindow);
        }

        private void ArtistFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = e.Source as TextBox;

            Dispatcher.Invoke(() =>
            {
                if (textBox != null) _artistFilter = textBox.Text;
                ArtistTreeView.ItemsSource = ArtistItems;
            });
        }

        private void DgTargetUpdated(object sender, DataTransferEventArgs e)
        {
            DataGrid dataGrid = UiHelpers.GetVisualParent<DataGrid>(sender);

            if (dataGrid == null) return;

            foreach (DataGridColumn column in dataGrid.Columns)
            {
                //if you want to size ur column as per the cell content
                column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells);
                //if you want to size ur column as per the column header
                column.Width = new DataGridLength(1, DataGridLengthUnitType.SizeToHeader);
                //if you want to size ur column as per both header and cell content
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }
    }
}
