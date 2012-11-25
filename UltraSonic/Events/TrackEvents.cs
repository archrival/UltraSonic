using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
    }
}
