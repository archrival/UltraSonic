using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Subsonic.Client.Models;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void TrackDataGridAddClick(object sender, RoutedEventArgs e)
        {
            foreach (TrackModel item in TrackDataGrid.SelectedItems)
                AddTrackItemToPlaylist(item);
        }

        private void TrackDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            DownloadTracks(TrackDataGrid.SelectedItems);
        }

        private void TrackDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid source = e.Source as DataGrid;
            if (source == null) return;
            if (_working) return;

            _working = true;

            TrackModel selectedTrack = source.CurrentItem as TrackModel;
            TrackModel playlistTrackItem = null;
            
            if (selectedTrack != null)
                playlistTrackItem = AddTrackItemToPlaylist(selectedTrack, _doubleClickBehavior == DoubleClickBehavior.Play);

            if (_doubleClickBehavior == DoubleClickBehavior.Play && playlistTrackItem != null)
            {
                if (PlaybackTrackGrid != null && _playbackTrackItems.Any())
                    PlaybackTrackGrid.SelectedItem = playlistTrackItem;

                StopMusic();
                PlayButtonClick(null, null);
            }

            _working = false;
        }

        private void PlayTrackImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow test = UiHelpers.GetVisualParent<DataGridRow>(sender);
            TrackModel trackItem = test.Item as TrackModel;

            if (trackItem == null) return;

            Dispatcher.Invoke(() => AddTrackToPlaylistAndPlay(trackItem));
        }

        private void AddTrackToPlaylistAndPlay(TrackModel trackItem)
        {
            TrackModel playlistTrackItem = AddTrackItemToPlaylist(trackItem, true);
            PlaylistTrackGrid.SelectedItem = playlistTrackItem;
            StopMusic();
            PlayButtonClick(null, null);
        }
    }
}
