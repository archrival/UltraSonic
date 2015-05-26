using Subsonic.Client;
using Subsonic.Client.Items;
using Subsonic.Common.Classes;
using Subsonic.Common.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdatePlaylists(Task<Playlists> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    UpdatePlaylists(task.Result.Items);
                    break;
            }
        }

        private void AddAlbumToPlaylist(Task<Directory> task, bool play = false, bool playback = false)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        if (playback)
                        {
                            StopButtonClick(null, null);
                            _playbackTrackItems.Clear();
                        }

                        foreach (TrackItem trackItem in GetTrackItemCollection(task.Result))
                            AddTrackItemToPlaylist(trackItem, playback, false);

                        if (!play) return;

                        PlaybackTrackGrid.SelectedIndex = 0;
                        PlayButtonClick(null, null);
                    });

                    break;
            }
        }

        private void UpdatePlaylistGrid(Task<PlaylistWithSongs> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                                          {
                                              _playlistTrackItems.Clear();

                                              foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                                              {
                                                  column.Width = column.MinWidth;
                                                  column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                                              }

                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result.Entries))
                                                  AddTrackItemToPlaylist(trackItem);
                                          });
                    break;
            }
        }

        private void UpdatePlaylistGrid(Task<Starred> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                                          {
                                              _playlistTrackItems.Clear();

                                              foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                                              {
                                                  column.Width = column.MinWidth;
                                                  column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                                              }

                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result.Songs))
                                                  AddTrackItemToPlaylist(trackItem);
                                          });
                    break;
            }
        }


        private void UpdatePlaylistGrid(Task<AlbumList> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        _playlistTrackItems.Clear();

                        foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                        {
                            column.Width = column.MinWidth;
                            column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                        }

                        var tracks = task.Result.Albums.Where(a => !a.IsDir).ToList();

                        foreach (TrackItem trackItem in GetTrackItemCollection(tracks))
                            AddTrackItemToPlaylist(trackItem);
                    });
                    break;
            }
        }
    
        private void RefreshStarredPlaylist(Task<bool> task, bool isTrack)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    if (task.Result && isTrack)
                        SubsonicClient.GetStarredAsync(null, GetCancellationToken("UpdatePlaylists")).ContinueWith(AddStarredToPlaylists);

                    break;
            } 
        }

        private void RefreshHighestRatedPlaylist(Task<bool> task, bool isTrack)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    if (task.Result && isTrack)
                        SubsonicClient.GetAlbumListAsync(AlbumListType.Highest, 500, null, null, null, null, null, GetCancellationToken("UpdatePlaylists")).ContinueWith(AddHighestRatedToPlaylists);

                    break;
            }
        }
        
        private void AddStarredToPlaylists(Task<Starred> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                                          {
                                              Starred starred = task.Result;
                                              int starDuration = starred.Songs.Sum(child => child.Duration);

                                              PlaylistItem newStarredPlaylist = new PlaylistItem
                                                                             {
                                                                                 Duration = TimeSpan.FromSeconds(starDuration),
                                                                                 Name = "Starred",
                                                                                 Tracks = starred.Songs.Count,
                                                                                 Playlist = null
                                                                             };

                                              PlaylistItem currentStarredPlaylist = _playlistItems.FirstOrDefault(p => p.Playlist == null && p.Name == "Starred");

                                              if (currentStarredPlaylist == null)
                                                  _playlistItems.Add(newStarredPlaylist);
                                              else
                                              {
                                                  _playlistItems.Remove(currentStarredPlaylist);
                                                  _playlistItems.Add(newStarredPlaylist);
                                              }
                                          });
                    break;
            }
        }

        private void AddHighestRatedToPlaylists(Task<AlbumList> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        AlbumList albumList = task.Result;
                        var tracks = albumList.Albums.Where(a => !a.IsDir).ToList();
                        int duration = tracks.Sum(child => child.Duration);

                        PlaylistItem newHighestRatedPlaylist = new PlaylistItem
                        {
                            Duration = TimeSpan.FromSeconds(duration),
                            Name = "Highest Rated",
                            Tracks = tracks.Count,
                            Playlist = null
                        };

                        PlaylistItem currentHighestRatedPlaylist = _playlistItems.FirstOrDefault(p => p.Playlist == null && p.Name == "Highest Rated");

                        if (currentHighestRatedPlaylist == null)
                            _playlistItems.Add(newHighestRatedPlaylist);
                        else
                        {
                            _playlistItems.Remove(currentHighestRatedPlaylist);
                            _playlistItems.Add(newHighestRatedPlaylist);
                        }
                    });
                    break;
            }
        }

        private void CheckPlaylistSave(Task<bool> task)
        {
            if (task.Status != TaskStatus.RanToCompletion || !task.Result) return;
            SubsonicClient.GetPlaylistsAsync(null, GetCancellationToken("CheckPlaylistSave")).ContinueWith(UpdatePlaylists);
        }
    }
}
