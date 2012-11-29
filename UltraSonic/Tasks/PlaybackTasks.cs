using System;
using System.Threading.Tasks;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void QueueTrack(Task task, TrackItem trackItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        DownloadStatusLabel.Content = string.Empty;

                        Uri thisUri;
                        _streamItems.TryDequeue(out thisUri);
                        trackItem.Cached = IsTrackCached(trackItem.FileName, trackItem.Child);
                        TrackDataGrid.Items.Refresh();
                        PlaylistTrackGrid.Items.Refresh();

                        QueueTrack(thisUri, trackItem);
                    });
                    break;
            }
        }

    }
}
