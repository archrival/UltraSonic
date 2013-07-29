using System.Windows.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Subsonic.Common;
using UltraSonic.Items;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdatePlaylists(Task<Playlists> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    UpdatePlaylists(task.Result.Playlist);
                    break;
            }
        }

        private void AddAlbumToPlaylist(Task<Directory> task, bool play = false)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                                          {
                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result))
                                                  AddTrackItemToPlaylist(trackItem);

                                              if (!play) return;

                                              PlaylistTrackGrid.SelectedIndex = 0;
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

                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result.Entry))
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

                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result.Song))
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
                        SubsonicClient.GetStarredAsync(GetCancellationToken("UpdatePlaylists")).ContinueWith(AddStarredToPlaylists);

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
                                              int starDuration = starred.Song.Sum(child => child.Duration);

                                              PlaylistItem newStarredPlaylist = new PlaylistItem
                                                                             {
                                                                                 Duration = TimeSpan.FromSeconds(starDuration),
                                                                                 Name = "Starred",
                                                                                 Tracks = starred.Song.Count,
                                                                                 Playlist = null
                                                                             };

                                              PlaylistItem currentStarredPlaylist = Enumerable.FirstOrDefault<PlaylistItem>(_playlistItems, p => p.Playlist == null);

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

        private void CheckPlaylistSave(Task<bool> task)
        {
            if (task.Status != TaskStatus.RanToCompletion || !task.Result) return;
            SubsonicClient.GetPlaylistsAsync(null, GetCancellationToken("CheckPlaylistSave")).ContinueWith(UpdatePlaylists);
        }
    }
}
