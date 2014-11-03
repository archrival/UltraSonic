using Subsonic.Client.Items;
using Subsonic.Common.Enums;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UltraSonic.Items;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void AlbumDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            var image = e.OriginalSource as Image;
            if (image != null && image.Name == "PlayTrackImage") return;
            if (SubsonicClient == null) return;

            var albumItem = AlbumDataGrid.SelectedItem as UltraSonicAlbumItem;

            if (albumItem != null)
                SubsonicClient.GetMusicDirectoryAsync(albumItem.Child.Id, GetCancellationToken("AlbumDataGridSelectionChanged")).ContinueWith(UpdateTrackListingGrid);
        }

        private async void AlbumDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SubsonicClient == null) return;
            if (_working) return;

            _working = true;

            var source = e.Source as DataGrid;
            if (source == null) return;

            var selectedAlbum = source.CurrentItem as UltraSonicAlbumItem;

            if (selectedAlbum != null)
                await SubsonicClient.GetMusicDirectoryAsync(selectedAlbum.Child.Id, GetCancellationToken("AlbumDataGridMouseDoubleClick")).ContinueWith(t => AddAlbumToPlaylist(t, _doubleClickBehavior == DoubleClickBehavior.Play, true));

            _working = false;
        }

        private void AlbumDataGridDownloadClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicClient == null) return;

            foreach (UltraSonicAlbumItem item in AlbumDataGrid.SelectedItems)
                Process.Start(SubsonicClient.BuildDownloadUrl(item.Child.Id).ToString());
        }

        private void AlbumDataGridAlbumListClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicClient == null) return;

            var source = e.Source as MenuItem;
            if (source == null) return;

            AlbumListType albumListType;

            switch (source.Header.ToString())
            {
                case "Newest":
                    albumListType = AlbumListType.Newest;
                    break;
                case "Random":
                    albumListType = AlbumListType.Random;
                    break;
                case "Highest Rated":
                    albumListType = AlbumListType.Highest;
                    break;
                case "Frequently Played":
                    albumListType = AlbumListType.Frequent;
                    break;
                case "Recently Played":
                    albumListType = AlbumListType.Recent;
                    break;
                case "Starred":
                    albumListType = AlbumListType.Starred;
                    break;
                default:
                    albumListType = AlbumListType.Newest;
                    break;
            }

            _albumListItem = new AlbumListItem {AlbumListType = albumListType, Current = 0};
            SubsonicClient.GetAlbumListAsync(albumListType, _albumListMax, null, null, null, null, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(t => UpdateAlbumGrid(t, _albumListMax + 1, _albumListMax + _albumListMax));
        }

        private void AlbumDataGridNextClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicClient == null || _albumListItem == null) return;

            _albumListItem.Current += _albumListMax;
            SubsonicClient.GetAlbumListAsync(_albumListItem.AlbumListType, _albumListMax, _albumListItem.Current, null, null, null, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(t => UpdateAlbumGrid(t, _albumListItem.Current + _albumListMax + 1, _albumListItem.Current + _albumListMax + _albumListMax));
        }

        private void AlbumDataGridAddClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicClient == null) return;

            foreach (UltraSonicAlbumItem item in AlbumDataGrid.SelectedItems)
                SubsonicClient.GetMusicDirectoryAsync(item.Child.Id, GetCancellationToken("AlbumDataGridAddClick")).ContinueWith(t => AddAlbumToPlaylist(t));
        }

        private void PlayAlbumImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var test = UiHelpers.GetVisualParent<DataGridRow>(sender);
            var albumItem = test.Item as UltraSonicAlbumItem;

            if (albumItem == null) return;

            Dispatcher.Invoke(() =>
                                  {
                                      StopMusic();

                                      _playbackTrackItems.Clear();

                                      foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                                      {
                                          column.Width = column.MinWidth;
                                          column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                                      }

                                      SubsonicClient.GetMusicDirectoryAsync(albumItem.Child.Id, GetCancellationToken("PlayAlbumImageMouseLeftButtonDown")).ContinueWith(t => AddAlbumToPlaylist(t, true, true));
                                  });
        }
    }
}
