﻿using System.Windows;
using Subsonic.Client.Activities;
using Subsonic.Client.Models;
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

            ArtistModel artistItem = e.NewValue as ArtistModel;
            _albumListItem = null;
            AlbumDataGridNext.Visibility = Visibility.Collapsed;

            if (artistItem == null || artistItem.Artist == null) return;

            ProgressIndicator.Visibility = Visibility.Visible;

            MusicDirectoryActivity<System.Drawing.Image> activity = new MusicDirectoryActivity<System.Drawing.Image>(SubsonicClient, artistItem.Artist.Id);
            await activity.GetResult(GetCancellationToken("ArtistTreeViewSelectionItemChanged")).ContinueWith(UpdateAlbumGrid);

            ProgressIndicator.Visibility = Visibility.Hidden;
        }
    }
}
