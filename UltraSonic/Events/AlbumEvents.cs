using Subsonic.Rest.Api.Enums;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void AlbumDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            if (SubsonicApi == null) return;

            AlbumItem albumItem = AlbumDataGrid.SelectedItem as AlbumItem;

            if (albumItem != null)
                SubsonicApi.GetMusicDirectoryAsync(albumItem.Child.Id, GetCancellationToken("AlbumDataGridSelectionChanged")).ContinueWith(UpdateTrackListingGrid);
        }

        private void AlbumDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SubsonicApi == null) return;

            DataGrid source = e.Source as DataGrid;
            if (source == null) return;

            AlbumItem selectedAlbum = source.CurrentItem as AlbumItem;

            if (selectedAlbum != null)
                SubsonicApi.GetMusicDirectoryAsync(selectedAlbum.Child.Id, GetCancellationToken("AlbumDataGridMouseDoubleClick")).ContinueWith(AddAlbumToPlaylist);
        }

        private void AlbumDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null) return;

            foreach (AlbumItem item in AlbumDataGrid.SelectedItems)
                Process.Start(SubsonicApi.BuildDownloadUrl(item.Child.Id));
        }

        private void AlbumDataGridAlbumListClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null) return;

            MenuItem source = e.Source as MenuItem;
            if (source == null) return;

            AlbumListType albumListType;

            switch (source.Header.ToString())
            {
                case "Newest":
                    albumListType = AlbumListType.newest;
                    break;
                case "Random":
                    albumListType = AlbumListType.random;
                    break;
                case "Highest Rated":
                    albumListType = AlbumListType.highest;
                    break;
                case "Frequently Played":
                    albumListType = AlbumListType.frequent;
                    break;
                case "Recently Played":
                    albumListType = AlbumListType.recent;
                    break;
                case "Starred":
                    albumListType = AlbumListType.starred;
                    break;
                default:
                    albumListType = AlbumListType.newest;
                    break;
            }

            _albumListItem = new AlbumListItem {Type = albumListType, Current = 0};
            AlbumDataGridNext.Header = string.Format("Albums {0} - {1}", _albumListMax + 1, _albumListMax + _albumListMax);
            AlbumDataGridNext.Visibility = Visibility.Visible;
            SubsonicApi.GetAlbumListAsync(albumListType, _albumListMax, null, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(UpdateAlbumGrid);
        }

        private void AlbumDataGridNextClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null || _albumListItem == null) return;

            _albumListItem.Current += _albumListMax;
            AlbumDataGridNext.Header = string.Format("Albums {0} - {1}", _albumListItem.Current + _albumListMax + 1, _albumListItem.Current + _albumListMax + _albumListMax);
            SubsonicApi.GetAlbumListAsync(_albumListItem.Type, _albumListMax, _albumListItem.Current, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(UpdateAlbumGrid);
        }

        private void AlbumDataGridAddClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null) return;

            foreach (AlbumItem item in AlbumDataGrid.SelectedItems)
                SubsonicApi.GetMusicDirectoryAsync(item.Child.Id, GetCancellationToken("AlbumDataGridAddClick")).ContinueWith(AddAlbumToPlaylist);
        }
    }
}
