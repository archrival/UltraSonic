﻿using System.Linq;
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
            if (_working) return;

            _working = true;

            TrackItem selectedTrack = source.CurrentItem as TrackItem;
            TrackItem playlistTrackItem = null;
            
            if (selectedTrack != null)
                playlistTrackItem = AddTrackItemToPlaylist(selectedTrack);

            if (_doubleClickBehavior == DoubleClickBehavior.Play && playlistTrackItem != null)
            {
                if (PlaylistTrackGrid != null && _playlistTrackItems.Any())
                    PlaylistTrackGrid.SelectedItem = playlistTrackItem;

                StopMusic();
                PlayButtonClick(null, null);
            }

            _working = false;
        }

        private void PlayTrackImageMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridRow test = UiHelpers.GetVisualParent<DataGridRow>(sender);
            TrackItem trackItem = test.Item as TrackItem;

            if (trackItem == null) return;

            Dispatcher.Invoke(() => AddTrackToPlaylistAndPlay(trackItem));
        }

        private void AddTrackToPlaylistAndPlay(TrackItem trackItem)
        {
            TrackItem playlistTrackItem = null;
            playlistTrackItem = AddTrackItemToPlaylist(trackItem);
            PlaylistTrackGrid.SelectedItem = playlistTrackItem;
            StopMusic();
            PlayButtonClick(null, null);
        }
    }
}
