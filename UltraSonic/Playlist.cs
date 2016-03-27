using Subsonic.Client.Models;
using Subsonic.Common;
using Subsonic.Common.Classes;
using Subsonic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdatePlaylists()
        {
            SubsonicClient?.GetPlaylistsAsync().ContinueWith(UpdatePlaylists, GetCancellationToken("UpdatePlaylists"));
        }

        private void UpdatePlaylists(IEnumerable<Playlist> playlists)
        {
            if (SubsonicClient == null) return;

            Dispatcher.Invoke(() =>
            {
                _playlistItems.Clear();
                
                foreach (DataGridColumn column in PlaylistsDataGrid.Columns)
                {
                    column.Width = column.MinWidth;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                foreach (PlaylistModel playlistItem in playlists.Select(playlist => new PlaylistModel
                {
                    Duration = TimeSpan.FromSeconds(playlist.Duration),
                    Name = playlist.Name,
                    Tracks = playlist.SongCount,
                    Playlist = playlist
                }))
                {
                    _playlistItems.Add(playlistItem);
                }
            
                if (SubsonicServer.ApiVersion >= SubsonicApiVersion.Version1_8_0) // Get starred tracks to create dynamic Starred playlist
                    SubsonicClient.GetStarredAsync(null, GetCancellationToken("UpdateStarredPlaylists")).ContinueWith(AddStarredToPlaylists);

                if (SubsonicServer.ApiVersion >= SubsonicApiVersion.Version1_2_0) // Get starred tracks to create dynamic Highest Rated playlist
                    SubsonicClient.GetAlbumListAsync(AlbumListType.Highest, 500, null, null, null, null, null, GetCancellationToken("UpdateHighestRatedPlaylists")).ContinueWith(AddHighestRatedToPlaylists);
            });
        }

        private TrackModel AddTrackItemToPlaylist(TrackModel trackItem, bool playback = false, bool clear = true)
        {
            var playlistTrackItem = new TrackModel();
            trackItem.CopyTo(playlistTrackItem);
            playlistTrackItem.Source = trackItem;
            playlistTrackItem.Duration = TimeSpan.FromSeconds(playlistTrackItem.Child.Duration);
            playlistTrackItem.PlaylistGuid = Guid.NewGuid();

            if (playback)
                Dispatcher.Invoke(() =>
                {
                    if (clear)
                        _playbackTrackItems.Clear();

                    _playbackTrackItems.Add(playlistTrackItem);

                });
            else
                Dispatcher.Invoke(() => _playlistTrackItems.Add(playlistTrackItem));

            return playlistTrackItem;
        }
    }
}
