using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PlaylistTrackGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_working) return;

            _working = true;

            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // Take first selected item and play it
            TrackItem playlistEntryItem = dataGrid.SelectedItems[0] as TrackItem;

            StopMusic();

            if (playlistEntryItem != null)
            {
                _shouldCachePlaylist = true;
                PlayTrack(playlistEntryItem);
            }

            _working = false;
        }

        private async void PlaylistsDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            if (_working) return;
            _working = true;

            PlaylistItem playlistItem = PlaylistsDataGrid.SelectedItem as PlaylistItem;

            if (playlistItem != null)
            {
                if (playlistItem.Playlist == null && playlistItem.Name == "Starred")
                {
                    await SubsonicApi.GetStarredAsync(GetCancellationToken("PlaylistsDataGridSelectionChanged")).ContinueWith(UpdatePlaylistGrid);
                }
                else
                {
                    CurrentPlaylist = playlistItem.Playlist;
                    if (playlistItem.Playlist != null) await SubsonicApi.GetPlaylistAsync(playlistItem.Playlist.Id, GetCancellationToken("PlaylistsDataGridSelectionChanged")).ContinueWith(UpdatePlaylistGrid);
                }
            }

            _working = false;
        }

        private void PlaylistTrackGridDownloadClick(object sender, RoutedEventArgs e)
        {
            DownloadTracks(PlaylistTrackGrid.SelectedItems);
        }

        private void PlaylistsDataGridRefreshClick(object sender, RoutedEventArgs e)
        {
            UpdatePlaylists();
        }

        private void PlaylistsDataGridDeletePlaylistClick(object sender, RoutedEventArgs e)
        {
            foreach (PlaylistItem playlistItem in PlaylistsDataGrid.SelectedItems)
            {
                if (playlistItem.Playlist == null && playlistItem.Name == "Starred")
                {
                    MessageBox.Show("Playlist 'Starred' is a dynamic playlist and cannot be deleted.", AppName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(string.Format("Would you like to delete the selected playlist? '{0}'", playlistItem.Name), AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes && playlistItem.Playlist != null)
                    {
                        // Unset current playlist if we are deleting it
                        if (CurrentPlaylist == playlistItem.Playlist)
                            CurrentPlaylist = null;

                        SubsonicApi.DeletePlaylistAsync(playlistItem.Playlist.Id).ContinueWith(t => UpdatePlaylists());
                    }
                }
            }
        }
    }
}
