using System.Windows;
using Subsonic.Client;
using Subsonic.Client.Items;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ExpandAllArtistsClick(object sender, RoutedEventArgs e)
        {
            ProgressIndicator.Visibility = Visibility.Visible;
            UiHelpers.ExpandAll(ArtistTreeView, true);
            ProgressIndicator.Visibility = Visibility.Hidden;
        }

        private void CollapseAllArtistsClick(object sender, RoutedEventArgs e)
        {
            ProgressIndicator.Visibility = Visibility.Visible;
            UiHelpers.ExpandAll(ArtistTreeView, false);
            ProgressIndicator.Visibility = Visibility.Hidden;
        }

        private void ArtistRefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateArtists();
        }

        private async void ArtistTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SubsonicClient == null) return;

            ArtistItem artistItem = e.NewValue as ArtistItem;
            _albumListItem = null;
            AlbumDataGridNext.Visibility = Visibility.Collapsed;

            if (artistItem == null || artistItem.Artist == null) return;

            ProgressIndicator.Visibility = Visibility.Visible;
            await SubsonicClient.GetMusicDirectoryAsync(artistItem.Artist.Id, GetCancellationToken("ArtistTreeViewSelectionItemChanged")).ContinueWith(UpdateAlbumGrid);
            ProgressIndicator.Visibility = Visibility.Hidden;
        }
    }
}
