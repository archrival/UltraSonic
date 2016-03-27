using System.Collections;
using System.Diagnostics;
using Subsonic.Client.Models;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void DownloadTracks(IEnumerable selectedItems)
        {
            if (SubsonicClient == null) return;

            foreach (TrackModel item in selectedItems)
                Process.Start(SubsonicClient.BuildDownloadUrl(item.Child.Id).ToString()); // Launch default URL handler for each download
        }
    }
}
