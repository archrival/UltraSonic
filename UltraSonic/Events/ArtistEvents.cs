using System.Windows;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ExpandAllArtistsClick(object sender, RoutedEventArgs e)
        {
            UiHelpers.ExpandAll(ArtistTreeView, true);
        }

        private void CollapseAllArtistsClick(object sender, RoutedEventArgs e)
        {
            UiHelpers.ExpandAll(ArtistTreeView, false);
        }

        private void ArtistRefreshClick(object sender, RoutedEventArgs e)
        {
            UpdateArtists();
        }

        private void ArtistTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SubsonicApi == null) return;

            ArtistItem artistItem = e.NewValue as ArtistItem;

            if (artistItem != null && artistItem.Artist != null)
                SubsonicApi.GetMusicDirectoryAsync(artistItem.Artist.Id, GetCancellationToken("ArtistTreeViewSelectionItemChanged")).ContinueWith(UpdateAlbumGrid);
        }
    }
}
