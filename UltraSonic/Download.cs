using System.Collections;
using System.Diagnostics;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void DownloadTracks(IEnumerable selectedItems)
        {
            if (SubsonicApi == null) return;

            foreach (TrackItem item in selectedItems)
                Process.Start(SubsonicApi.BuildDownloadUrl(item.Child.Id)); // Launch default URL handler for each download
        }
    }
}
