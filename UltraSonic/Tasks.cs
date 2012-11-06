using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Subsonic.Rest.Api;

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
                        Image coverArtimage = task.Result;
        
                        if (coverArtimage != null)
                        {
                            albumItem.Image = coverArtimage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                            MusicDataGrid.Items.Refresh();
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

        private void QueueTrack(Task<long> task, Child child)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        Uri thisUri;
                        _streamItems.TryDequeue(out thisUri);

                        QueueTrack(thisUri, child);
                    });
            }
        }

        private void UpdateCoverArt(Task<Image> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Image coverArtimage = task.Result;

                if (coverArtimage != null)
                    Dispatcher.Invoke(() => MusicCoverArt.Source = coverArtimage.ToBitmapSource());
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
                            _playlistTrackItems.Add(trackItem);

                        PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                    });
            }
        }

        private void UpdatePlaylistGrid(Task<PlaylistWithSongs> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        _playlistTrackItems.Clear();

                        foreach (var t in GetTrackItemCollection(task.Result.Entry))
                            _playlistTrackItems.Add(t);

                        PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
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
                            _playlistTrackItems.Add(t);

                        PlaylistTrackGrid.ItemsSource = _playlistTrackItems;
                    });
            }
        }

        private void PopulateSearchResults(Task<SearchResult2> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                SearchResult2 searchResult = task.Result;

                UpdateAlbumGrid(searchResult.Album);
                UpdateTrackListingGrid(searchResult.Song);
            }
        }

        private void UpdateCurrentUser(Task<User> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                CurrentUser = task.Result;

                Dispatcher.Invoke(() =>
                    {
                        DownloadsTab.Visibility = CurrentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
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