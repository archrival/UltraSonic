using System.Collections;
using System.Diagnostics;
using UltraSonic.Items;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void DownloadTracks(IEnumerable selectedItems)
        {
            if (SubsonicClient == null) return;

            //foreach (TrackItem item in selectedItems)
            //   Process.Start(SubsonicClient.BuildDownloadUrl(item.Child.Id)); // Launch default URL handler for each download
        }
    }
}
