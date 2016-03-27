﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using Subsonic.Client.Activities;
using Subsonic.Client.Models;
using UltraSonic.Properties;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ShuffleButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.Invoke(() => _playbackTrackItems.Shuffle());
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
                                          if (PlaybackTrackGrid.Items.Count > 0)
                                          {
                                              foreach (TrackModel trackItem in PlaybackTrackGrid.Items.Cast<TrackModel>().Where(trackItem => !trackItem.Cached))
                                              {
                                                  _shouldCachePlaylist = true;
                                                  break;
                                              }

                                              if (PlaybackTrackGrid.SelectedIndex == -1)
                                                  PlaybackTrackGrid.SelectedIndex = 0;

                                              var playlistEntryItem = PlaybackTrackGrid.SelectedItem as TrackModel;

                                              if (playlistEntryItem != null)
                                                  QueueTrack(playlistEntryItem);
                                          }
                                      }
                                  });
        }

        private async void CachePlaylistTracks()
        {
            if (_caching) return;

            List<TrackModel> playlistTracks = CollectionViewSource.GetDefaultView(_playbackTrackItems).Cast<TrackModel>().ToList();

            foreach (TrackModel trackItem in playlistTracks)
            {
                if (IsTrackCached(trackItem.FileName, trackItem.Child)) continue;
                if (_playbackTrackItems.All(t => t != trackItem)) continue;
                if (!_shouldCachePlaylist) break;

                await _cachingThrottle.WaitAsync();

                try
                {
                    Uri uri = new Uri(trackItem.FileName);
                    if (_streamItems.Any(s => s == uri)) continue;

                    CancellationToken token = GetCancellationToken("CachePlaylistTracks");

                    _caching = true;

                    DownloadStatusLabel.Content = $"Caching: {trackItem.Child.Title}";
                    TrackModel item = trackItem;
                    await SubsonicClient.StreamAsync(trackItem.Child.Id, trackItem.FileName, _streamParameters, null, null, null, null, token).ContinueWith(t => FinalizeCache(t, item), token);
                }
                finally
                {
                    _cachingThrottle.Release();
                }
            }
        }

        private void FinalizeCache(Task<long> task, TrackModel trackItem)
        {
           Dispatcher.Invoke(() => DownloadStatusLabel.Content = string.Empty);

           switch (task.Status)
           {
               case TaskStatus.RanToCompletion:
                   Dispatcher.Invoke(() =>
                                         {
                                             trackItem.Cached = IsTrackCached(trackItem.FileName, trackItem.Child);
                                             if (trackItem.Source != null) trackItem.Source.Cached = trackItem.Cached;
                                             _caching = false;
                                         });
                   break;
           }
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
            _shouldCachePlaylist = false;
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

        private void ProgressSliderMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _movingSlider = true;
        }

        private void ProgressSliderMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int sliderValue = (int)ProgressSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, sliderValue);
            MediaPlayer.Position = ts;
            _movingSlider = false;
        }

        private void SavePlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicClient == null) return;

            bool updatePlaylist = false;

            if (CurrentPlaylist != null)
            {
                MessageBoxResult result = MessageBox.Show($"Would you like to update the previously loaded playlist? '{CurrentPlaylist.Name}'", AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                updatePlaylist = (result == MessageBoxResult.Yes);
            }

            if (updatePlaylist)
            {
                Dispatcher.Invoke(() =>
                {
                    List<string> playlistTracks = new List<string>();
                    playlistTracks.AddRange(_playlistTrackItems.Select(test => test.Child.Id));

                    SubsonicClient.CreatePlaylistAsync(CurrentPlaylist.Id, null, playlistTracks).ContinueWith(CheckPlaylistSave);
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

                    SubsonicClient.CreatePlaylistAsync(null, playlistName, playlistTracks).ContinueWith(CheckPlaylistSave);
                });
            }
        }

        private void NewPlaylistButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to clear the current playlist?", AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                Dispatcher.Invoke(() =>
                {
                    CurrentPlaylist = null;
                    _playlistTrackItems.Clear();

                    foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                    {
                        column.Width = column.MinWidth;
                        column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                    }
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
            Settings.Default.ShowAlbumArt = _showAlbumArt;

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

            if (_savePlaybackList)
            {
                string playlistXml;

                using (StringWriter textWriter = new StringWriter())
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(_playbackTrackItems.GetType());
                    XmlWriter writer = XmlWriter.Create(textWriter);
                    xmlSerializer.Serialize(writer, _playbackTrackItems);
                    playlistXml = textWriter.ToString();
                }

                Settings.Default.CurrentPlaybackList = playlistXml;
            }
            else
            {
                Settings.Default.CurrentPlaybackList = string.Empty;
            }

            Settings.Default.Save();

            if (StreamProxy == null) return;

            StreamProxy.Stop();
            StreamProxy = null;
        }

        private async void GlobalSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;

            string searchQuery = GlobalSearchTextBox.Text;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                _albumItems.Clear();
                _trackItems.Clear();

                foreach (DataGridColumn column in AlbumDataGrid.Columns)
                {
                    column.Width = column.MinWidth;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                foreach (DataGridColumn column in TrackDataGrid.Columns)
                {
                    column.Width = column.MinWidth;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                SearchStatusLabel.Content = "Searching...";

                Search2Activity<System.Drawing.Image> search2Activity = new Search2Activity<System.Drawing.Image>(SubsonicClient, searchQuery, _maxSearchResults, 0, _maxSearchResults, 0, _maxSearchResults, 0, null);
                await search2Activity.GetResult(GetCancellationToken("GlobalSearchTextBoxKeyDown")).ContinueWith(PopulateSearchResults);
            }
        }

        private void ArtistFilterTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;

            Dispatcher.Invoke(() => UiHelpers.ExpandAll(ArtistTreeView, true));
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

            try
            {
                if (_currentAlbumArt.Height > ActualHeight * 0.9)
                {
                    var newHeight = (int)(ActualHeight * 0.9);
                    bitmap = _currentAlbumArt.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, 0, newHeight);
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
            catch
            { }
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
    }
}
