using System.Windows.Controls;
using Subsonic.Client.Models;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void ChangeItemRating(RatingControl ratingControl)
        {
            if (SubsonicClient == null || ratingControl == null) return;

            // Find parent DataGrid, if any
            DataGrid dataGrid = UiHelpers.TryFindParent<DataGrid>(ratingControl);
            if (dataGrid == null) return;

            // Take first selected item in parent DataGrid, if any
            ChildModel childItem = dataGrid.SelectedItem as ChildModel;
            if (childItem == null) return;

            SubsonicClient.SetRatingAsync(childItem.Child.Id, ratingControl.RatingValue).ConfigureAwait(false);
        }
    }
}
