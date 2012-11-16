using System.IO;
using Subsonic.Rest.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Directory = Subsonic.Rest.Api.Directory;
using Image = System.Drawing.Image;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateAlbumGrid(Task<Directory> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
                UpdateAlbumGrid(task.Result.Child.Where(child => child.IsDir));
        }

        private void UpdateAlbumGrid(Task<AlbumList> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
                UpdateAlbumGrid(task.Result.Album);
        }

        private void UpdateAlbumImageArt(Task<Image> task, AlbumItem albumItem)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        Image coverArtImage = task.Result;
        
                        if (coverArtImage != null)
                        {
                            string localFileName = GetCoverArtFilename(albumItem.Album);
                            coverArtImage.Save(localFileName);

                            albumItem.Image = coverArtImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                            MusicDataGrid.Items.Refresh();
                            coverArtImage.Dispose();
                        }
                    });
            }
        }

        private void UpdateNowPlayingAlbumImageArt(Task<Image> task, NowPlayingItem nowPlayingItem)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                {
                    Image coverArtImage = task.Result;

                    if (coverArtImage != null)
                    {
                        string localFileName = GetCoverArtFilename(nowPlayingItem.Track);
                        coverArtImage.Save(localFileName);

                        nowPlayingItem.Image = coverArtImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                        NowPlayingDataGrid.Items.Refresh();
                    }
                });
            }
        }

        private void UpdateTrackListingGrid(Task<Directory> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
                UpdateTrackListingGrid(task.Result.Child);
        }

        private void UpdateTrackListingGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() => TrackDataGrid.ItemsSource = GetTrackItemCollection(children));
        }

        private void QueueTrack(Task<long> task, TrackItem trackItem)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      DownloadStatusLabel.Content = string.Empty;

                                      if (task.Status == TaskStatus.RanToCompletion)
                                      {
                                          Uri thisUri;
                                          _streamItems.TryDequeue(out thisUri);

                                          QueueTrack(thisUri, trackItem);
                                      }
                                  });
        }

        private void UpdateCoverArt(Task<Image> task, Child child)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                _currentAlbumArt = task.Result;

                if (_currentAlbumArt != null)
                {
                    string localFileName = GetCoverArtFilename(child);
                    _currentAlbumArt.Save(localFileName);

                    Dispatcher.Invoke(() => MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int) MusicCoverArt.Width, (int) MusicCoverArt.Height));
                }
            }
        }

        private void UpdateArtistsTreeView(Task<Indexes> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                                      {
                                          ArtistItems.Clear();

                                          foreach (Index index in task.Result.Index)
                                          {
                                              ArtistItem artistItem = new ArtistItem { Name = index.Name };

                                              foreach (Artist artist in index.Artist)
                                                  artistItem.Children.Add(new ArtistItem { Name = artist.Name, Artist = artist });

                                              ArtistItems.Add(artistItem);
                                          }
                                      });
            }
        }

        private void CheckPlaylistSave(Task<bool> task)
        {
            if (task.Status == TaskStatus.RanToCompletion && task.Result)
            {
                Dispatcher.Invoke(() =>
                    {
                        SubsonicApi.GetPlaylistsAsync(null, GetCancellationToken("CheckPlaylistSave")).ContinueWith(UpdatePlaylists);
                    });
            }
        }

        private void UpdatePlaylists(Task<Playlists> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
                UpdatePlaylists(task.Result.Playlist);
        }

        private void AddAlbumToPlaylist(Task<Directory> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        IEnumerable<TrackItem> albumSource = GetTrackItemCollection(task.Result);

                        foreach (var trackItem in albumSource)
                            AddTrackItemToPlaylist(trackItem);
                    });
            }
        }

        private void AddTrackItemToPlaylist(TrackItem trackItem)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      TrackItem playlistTrackItem = new TrackItem();
                                      trackItem.CopyTo(playlistTrackItem);
                                      playlistTrackItem.PlaylistGuid = Guid.NewGuid();

                                      _playlistTrackItems.Add(playlistTrackItem);
                                  });
        }

        private void UpdatePlaylistGrid(Task<PlaylistWithSongs> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        _playlistTrackItems.Clear();

                        foreach (var t in GetTrackItemCollection(task.Result.Entry))
                            AddTrackItemToPlaylist(t);
                    });
            }
        }

        private void UpdatePlaylistGrid(Task<Starred> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        _playlistTrackItems.Clear();

                        foreach (var t in GetTrackItemCollection(task.Result.Song))
                            AddTrackItemToPlaylist(t);
                    });
            }
        }

        private void PopulateSearchResults(Task<SearchResult2> task)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      SearchStatusLabel.Content = string.Empty;

                                      if (task.Status == TaskStatus.RanToCompletion)
                                      {
                                          SearchResult2 searchResult = task.Result;

                                          UpdateAlbumGrid(searchResult.Album);
                                          UpdateTrackListingGrid(searchResult.Song);
                                      }
                                  });
        }

        private void UpdateNowPlaying(Task<NowPlaying> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                                      {
                                          foreach (var item in task.Result.Entry)
                                          {
                                              NowPlayingItem nowPlayingItem = new NowPlayingItem
                                                                                  {
                                                                                      BitRate = item.BitRate,
                                                                                      DiscNumber = item.DiscNumber,
                                                                                      Duration = TimeSpan.FromSeconds(item.Duration),
                                                                                      Genre = item.Genre,
                                                                                      TrackNumber = item.Track,
                                                                                      Rating = item.UserRating,
                                                                                      Year = item.Year,
                                                                                      Album = item.Album,
                                                                                      Artist = item.Artist,
                                                                                      Track = item,
                                                                                      Starred = (item.Starred != default(DateTime)),
                                                                                      Title = item.Title,
                                                                                      User = item.Username,
                                                                                      When = (DateTime.Now - TimeSpan.FromMinutes(item.MinutesAgo)).ToShortTimeString()
                                                                                  };

                                              if (!_nowPlayingItems.Any(a => a.Album == nowPlayingItem.Album && a.Artist == nowPlayingItem.Artist && a.Starred == nowPlayingItem.Starred && a.Title == nowPlayingItem.Title))
                                              {
                                                  _nowPlayingItems.Add(nowPlayingItem);

                                                  string localFileName = GetCoverArtFilename(item);
                                                  if (File.Exists(localFileName))
                                                  {
                                                      Image thisImage = Image.FromFile(localFileName);
                                                      Dispatcher.Invoke(() =>
                                                                            {
                                                                                nowPlayingItem.Image = thisImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                                                                                NowPlayingDataGrid.Items.Refresh();
                                                                            });
                                                  }
                                                  else
                                                  {
                                                      SubsonicApi.GetCoverArtAsync(item.CoverArt).ContinueWith(t => UpdateNowPlayingAlbumImageArt(t, nowPlayingItem));
                                                  }
                                              }
                                          }
                                      });
            }
        }

        private void UpdateChatMessages(Task<ChatMessages> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                                      {
                                          TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                                          _chatMessageSince = t.TotalMilliseconds;

                                          foreach (var item in task.Result.ChatMessage.OrderBy(c => c.Time))
                                          {
                                              DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                              dtDateTime = dtDateTime.AddMilliseconds(item.Time).ToLocalTime();

                                              if (!_chatMessages.Any(c => c.TimeStamp == dtDateTime && c.Message == item.Message && c.User == item.Username))
                                              {
                                                  if (!SocialTab.IsSelected && _newChatNotify)
                                                      SocialTab.SetValue(StyleProperty, Resources["FlashingHeader"]);

                                                  _chatMessages.Add(new ChatItem {User = item.Username, Message = item.Message, TimeStamp = dtDateTime});
                                              }
                                          }

                                          _newChatNotify = true;
                                      });
            }
        }

        private void UpdateCurrentUser(Task<User> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                CurrentUser = task.Result;

                Dispatcher.Invoke(() =>
                    {
                        MusicDataGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        TrackDataGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PlaylistTrackGridDownload.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PreviousButton.IsEnabled = CurrentUser.StreamRole;
                        PlayButton.IsEnabled = CurrentUser.StreamRole;
                        StopButton.IsEnabled = CurrentUser.StreamRole;
                        PauseButton.IsEnabled = CurrentUser.StreamRole;
                        NextButton.IsEnabled = CurrentUser.StreamRole;
                        SavePlaylistButton.IsEnabled = CurrentUser.PlaylistRole;
                        PlaylistsGridDeletePlaylist.Visibility = CurrentUser.PlaylistRole ? Visibility.Visible : Visibility.Collapsed;

                        UserEmailLabel.Content = CurrentUser.Email;
                        UserScrobblingLabel.Content = CurrentUser.ScrobblingEnabled;
                        UserAdminLabel.Content = CurrentUser.AdminRole;
                        UserSettingsLabel.Content = CurrentUser.SettingsRole;
                        UserStreamLabel.Content = CurrentUser.StreamRole;
                        UserJukeboxLabel.Content = CurrentUser.JukeboxRole;
                        UserDownloadLabel.Content = CurrentUser.DownloadRole;
                        UserUploadLabel.Content = CurrentUser.UploadRole;
                        UserPlaylistLabel.Content = CurrentUser.PlaylistRole;
                        UserCoverArtLabel.Content = CurrentUser.CoverArtRole;
                        UserCommentLabel.Content = CurrentUser.CommentRole;
                        UserPodcastLabel.Content = CurrentUser.PodcastRole;
                        UserShareLabel.Content = CurrentUser.ShareRole;

                        if (SubsonicApi.ServerApiVersion >= Version.Parse("1.8.0"))
                            SubsonicApi.GetAvatarAsync(CurrentUser.Username, GetCancellationToken("UpdateCurrentUser")).ContinueWith(UpdateUserAvatar);
                        else
                            UserAvatarImage.Visibility = Visibility.Collapsed;
                    });
            }
        }

        private void UpdateUserAvatar(Task<Image> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                {
                    Image avatarImage = task.Result;

                    if (avatarImage != null)
                        UserAvatarImage.Source = task.Result.ToBitmapSource();
                });
            }
        }
    }
}