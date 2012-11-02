using Subsonic.Rest.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private ObservableCollection<AlbumItem> _albumItems = new ObservableCollection<AlbumItem>();

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
                        albumItem.Image = task.Result.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, 200, 200);
                        MusicDataGrid.Items.Refresh();
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
                        _streams.TryDequeue(out thisUri);

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
                            ArtistItem artistItem = new ArtistItem {Name = index.Name};

                            foreach (Artist artist in index.Artist)
                                artistItem.Children.Add(new ArtistItem {Name = artist.Name, Artist = artist});

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
                        SubsonicApi.GetPlaylistsAsync().ContinueWith(UpdatePlaylists);
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
                        ObservableCollection<TrackItem> itemsSource = PlaylistTrackGrid.ItemsSource as ObservableCollection<TrackItem> ?? new ObservableCollection<TrackItem>();
                        IEnumerable<TrackItem> albumSource = GetTrackItemCollection(task.Result);

                        foreach (var trackItem in albumSource)
                            itemsSource.Add(trackItem);

                        PlaylistTrackGrid.ItemsSource = itemsSource;
                    });
            }
        }

        private void UpdatePlaylistGrid(Task<PlaylistWithSongs> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        PlaylistTrackGrid.ItemsSource = GetTrackItemCollection(task.Result.Entry);
                    });
            }
        }

        private void UpdatePlaylistGrid(Task<Starred> task)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                Dispatcher.Invoke(() =>
                    {
                        PlaylistTrackGrid.ItemsSource = GetTrackItemCollection(task.Result.Song);
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
                _currentUser = task.Result;

                Dispatcher.Invoke(() =>
                    {
                        DownloadsTab.Visibility = _currentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        MusicDataGridDownload.Visibility = _currentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        TrackDataGridDownload.Visibility = _currentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PlaylistTrackGridDownload.Visibility = _currentUser.DownloadRole ? Visibility.Visible : Visibility.Collapsed;
                        PreviousButton.IsEnabled = _currentUser.StreamRole;
                        PlayButton.IsEnabled = _currentUser.StreamRole;
                        StopButton.IsEnabled = _currentUser.StreamRole;
                        PauseButton.IsEnabled = _currentUser.StreamRole;
                        NextButton.IsEnabled = _currentUser.StreamRole;
                        SavePlaylistButton.IsEnabled = _currentUser.PlaylistRole;
                        PlaylistsGridDeletePlaylist.Visibility = _currentUser.PlaylistRole ? Visibility.Visible : Visibility.Collapsed;
                    });
            }
        }
    }
}