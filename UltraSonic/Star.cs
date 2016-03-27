using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Subsonic.Client.Models;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ChangeItemStarState(ToggleButton toggleButton)
        {
            if (SubsonicClient == null || toggleButton == null) return;

            // Find parent DataGrid, if any
            DataGrid dataGrid = UiHelpers.TryFindParent<DataGrid>(toggleButton);
            if (dataGrid == null) return;

            // Take first selected item in parent DataGrid, if any
            ChildModel childItem = dataGrid.SelectedItem as ChildModel;
            if (childItem == null) return;

            if (toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value)
                SubsonicClient.StarAsync(new List<string> { childItem.Child.Id }).ContinueWith(t => RefreshStarredPlaylist(t, !childItem.Child.IsDir));
            else
                SubsonicClient.UnStarAsync(new List<string> { childItem.Child.Id }).ContinueWith(t => RefreshStarredPlaylist(t, !childItem.Child.IsDir));
        }
    }
}
