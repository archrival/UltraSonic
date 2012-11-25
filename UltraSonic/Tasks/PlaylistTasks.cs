using Subsonic.Rest.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

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

                private void AddAlbumToPlaylist(Task<Directory> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                                          {
                                              foreach (TrackItem trackItem in GetTrackItemCollection(task.Result))
                                                  AddTrackItemToPlaylist(trackItem);
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
                        SubsonicApi.GetStarredAsync(GetCancellationToken("UpdatePlaylists")).ContinueWith(AddStarredToPlaylists);

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

                                              PlaylistItem currentStarredPlaylist = _playlistItems.FirstOrDefault(p => p.Playlist == null);

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
            SubsonicApi.GetPlaylistsAsync(null, GetCancellationToken("CheckPlaylistSave")).ContinueWith(UpdatePlaylists);
        }
    }
}
