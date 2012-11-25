using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void QueueTrack(Task<long> task, TrackItem trackItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        DownloadStatusLabel.Content = string.Empty;

                        Uri thisUri;
                        _streamItems.TryDequeue(out thisUri);

                        QueueTrack(thisUri, trackItem);
                    });
                    break;
            }
        }

    }
}
