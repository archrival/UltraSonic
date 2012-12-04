using System.Linq;
using Subsonic.Rest.Api.Enums;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void AlbumDataGridSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            Image image = e.OriginalSource as Image;
            if (image != null && image.Name == "PlayTrackImage") return;
            if (SubsonicApi == null) return;

            AlbumItem albumItem = AlbumDataGrid.SelectedItem as AlbumItem;

            if (albumItem != null)
                SubsonicApi.GetMusicDirectoryAsync(albumItem.Child.Id, GetCancellationToken("AlbumDataGridSelectionChanged")).ContinueWith(UpdateTrackListingGrid);
        }

        private async void AlbumDataGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SubsonicApi == null) return;
            if (_working) return;

            _working = true;

            DataGrid source = e.Source as DataGrid;
            if (source == null) return;

            AlbumItem selectedAlbum = source.CurrentItem as AlbumItem;

            if (selectedAlbum != null)
                await SubsonicApi.GetMusicDirectoryAsync(selectedAlbum.Child.Id, GetCancellationToken("AlbumDataGridMouseDoubleClick")).ContinueWith(t => AddAlbumToPlaylist(t, _doubleClickBehavior == DoubleClickBehavior.Play));

            _working = false;
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
            SubsonicApi.GetAlbumListAsync(albumListType, _albumListMax, null, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(t => UpdateAlbumGrid(t, _albumListMax + 1, _albumListMax + _albumListMax));
        }

        private void AlbumDataGridNextClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null || _albumListItem == null) return;

            _albumListItem.Current += _albumListMax;
            SubsonicApi.GetAlbumListAsync(_albumListItem.Type, _albumListMax, _albumListItem.Current, GetCancellationToken("AlbumDataGridAlbumListClick")).ContinueWith(t => UpdateAlbumGrid(t, _albumListItem.Current + _albumListMax + 1, _albumListItem.Current + _albumListMax + _albumListMax));
        }

        private void AlbumDataGridAddClick(object sender, RoutedEventArgs e)
        {
            if (SubsonicApi == null) return;

            foreach (AlbumItem item in AlbumDataGrid.SelectedItems)
                SubsonicApi.GetMusicDirectoryAsync(item.Child.Id, GetCancellationToken("AlbumDataGridAddClick")).ContinueWith(t => AddAlbumToPlaylist(t));
        }

        private void PlayAlbumImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow test = UiHelpers.GetVisualParent<DataGridRow>(sender);
            AlbumItem albumItem = test.Item as AlbumItem;

            if (albumItem == null) return;

            StopMusic();

            Dispatcher.Invoke(() =>
                                  {
                                      if (_albumPlayButtonBehavior == AlbumPlayButtonBehavior.Ask && _playlistTrackItems.Any())
                                      {
                                          MessageBoxResult result = MessageBox.Show("Would you like to save the current playlist?", AppName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                                          if (result == MessageBoxResult.Yes)
                                              SavePlaylistButtonClick(null, null);
                                      }

                                      _playlistTrackItems.Clear();

                                      foreach (DataGridColumn column in PlaylistTrackGrid.Columns)
                                      {
                                          column.Width = column.MinWidth;
                                          column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                                      }

                                      SubsonicApi.GetMusicDirectoryAsync(albumItem.Child.Id, GetCancellationToken("PlayAlbumImageMouseLeftButtonDown")).ContinueWith(t => AddAlbumToPlaylist(t, true));
                                  });
        }
    }
}
