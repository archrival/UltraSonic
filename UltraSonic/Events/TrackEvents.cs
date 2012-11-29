using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void TrackDataGridAddClick(object sender, RoutedEventArgs e)
        {
            foreach (TrackItem item in TrackDataGrid.SelectedItems)
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

            TrackItem selectedTrack = source.CurrentItem as TrackItem;

            if (selectedTrack != null)
                AddTrackItemToPlaylist(selectedTrack);
        }

        private void PlayTrackImageMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow test = UiHelpers.GetVisualParent<DataGridRow>(sender);
            TrackItem trackItem = test.Item as TrackItem;

            if (trackItem == null) return;

            Dispatcher.Invoke(() =>
            {
                _playlistTrackItems.Clear();
                _playlistTrackItems.Add(trackItem);
                PlaylistTrackGrid.SelectedIndex = 0;
                PlayButtonClick(null, null);
            });
        }
    }
}
