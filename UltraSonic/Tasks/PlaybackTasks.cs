﻿using System;
using System.Threading.Tasks;
using Subsonic.Client.Models;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void QueueTrack(Task task, TrackModel trackItem)
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
                                              _caching = false;

                                              if (trackItem.Source != null)
                                                  trackItem.Source.Cached = trackItem.Cached;

                                              //QueueTrack(thisUri, trackItem);
                                          });
                    break;
            }
        }
    }
}
