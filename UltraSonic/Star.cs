using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ChangeItemStarState(ToggleButton toggleButton)
        {
            if (SubsonicApi == null || toggleButton == null) return;

            // Find parent DataGrid, if any
            DataGrid dataGrid = UiHelpers.TryFindParent<DataGrid>(toggleButton);
            if (dataGrid == null) return;

            // Take first selected item in parent DataGrid, if any
            ChildItem childItem = dataGrid.SelectedItem as ChildItem;
            if (childItem == null) return;

            if (toggleButton.IsChecked.HasValue && toggleButton.IsChecked.Value)
                SubsonicApi.StarAsync(new List<string> {childItem.Child.Id});
            else
                SubsonicApi.UnStarAsync(new List<string> {childItem.Child.Id});
        }
    }
}
