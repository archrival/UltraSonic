using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows;
using Subsonic.Client.Common.Items;
using Subsonic.Common;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PlayNextTrack()
        {
            Dispatcher.Invoke(() =>
            {
                bool playNextTrack = false;
                int playlistTrack = _nowPlayingTrack != null ? _playlistTrackItems.IndexOf(_nowPlayingTrack) : PlaylistTrackGrid.SelectedIndex;

                if (playlistTrack == _playlistTrackItems.Count - 1)
                {
                    if (_repeatPlaylist)
                    {
                        if (_playbackFollowsCursor)
                            PlaylistTrackGrid.SelectedIndex = 0;

                        playNextTrack = true;
                    }
                }
                else
                {
                    if (_playbackFollowsCursor)
                        PlaylistTrackGrid.SelectedIndex = playlistTrack + 1;

                    playNextTrack = true;
                }

                StopMusic();

                if (playNextTrack)
                    PlayTrack(_playlistTrackItems[playlistTrack + 1]);
            });
        }

        private void PlayPreviousTrack()
        {
            Dispatcher.Invoke(() =>
            {
                int playlistTrack = _nowPlayingTrack != null ? _playlistTrackItems.IndexOf(_nowPlayingTrack) : PlaylistTrackGrid.SelectedIndex;

                if (_playbackFollowsCursor)
                    PlaylistTrackGrid.SelectedIndex--;

                StopMusic();

                PlayTrack(_playlistTrackItems[playlistTrack - 1]);
            });
        }

        private void PlayMusic()
        {
            if (MediaPlayer.Source != null)
                MediaPlayer.Play();

            MusicPlayStatusLabel.Content = "Playing";
        }

        private void PauseMusic()
        {
            if (MediaPlayer.Source != null)
                MediaPlayer.Pause();

            MusicPlayStatusLabel.Content = "Paused";
        }

        private void StopMusic()
        {
            if (MediaPlayer.Source != null)
            {
                MediaPlayer.Stop();
                MediaPlayer.Position = TimeSpan.FromSeconds(0);
                MediaPlayer.Source = null;
                _nowPlayingTrack = null;
                _position = TimeSpan.FromSeconds(0);
                _currentAlbumArt = null;
                MusicCoverArt.Source = null;
                MusicArtistLabel.Text = null;
                MusicAlbumLabel.Text = null;
                MusicTitleLabel.Text = null;
                ProgressSlider.Value = MediaPlayer.Position.TotalMilliseconds;
                MusicTimeRemainingLabel.Content = string.Format("{0:mm\\:ss} / {1:mm\\:ss}", TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds), TimeSpan.FromMilliseconds(_position.TotalMilliseconds));
                UpdateTitle();

            }

            MusicPlayStatusLabel.Content = "Stopped";
        }

        private void QueueTrack(TrackItem trackItem)
        {
            if (_streamItems == null) return;

            Child child = trackItem.Child;             
            Uri fileNameUri = new Uri(trackItem.FileName);

            if (_streamItems.All(s => s.OriginalString == trackItem.FileName) && trackItem.Cached)
            {
                // Use this to tell Subsonic we're playing back the track, this will result in the server indicating we have cancelled the data transfer, it isn't very nice.
                //SubsonicClient.StreamAsync(child.Id, trackItem.FileName, _maxBitrate == 0 ? null : (int?) _maxBitrate, null, null, null, null, GetCancellationToken("QueueTrack"), true);
                QueueTrackItemForPlayback(trackItem, false);
                UpdateAlbumArt(child);
            }
            else
            {
                _streamItems.Enqueue(fileNameUri);
                UpdateAlbumArt(child);

                _caching = true;
                DownloadStatusLabel.Content = string.Format("Caching: {0}", child.Title);
                var streamTask = SubsonicClient.StreamAsync(child.Id, trackItem.FileName, _maxBitrate == 0 ? null : (int?) _maxBitrate, null, null, null, null, GetCancellationToken("QueueTrack"));
                QueueTrackItemForPlayback(trackItem, true);
                streamTask.ContinueWith(t => QueueTrack(t, trackItem));

                //if (_useDiskCache)
                //{
                //    _caching = true;
                //    DownloadStatusLabel.Content = string.Format("Caching: {0}", child.Title);
                //    SubsonicClient.StreamAsync(child.Id, trackItem.FileName, _maxBitrate == 0 ? null : (int?)_maxBitrate, null, null, null, null, GetCancellationToken("QueueTrack")).ContinueWith(t => QueueTrack(t, trackItem));
                //}
                //else
                //{
                //    QueueTrack(new Uri(SubsonicClient.BuildStreamUrl(child.Id)), trackItem); // Works with non-SSL servers
                //}
            }
        }

        private void QueueTrackItemForPlayback(TrackItem trackItem, bool stream)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    StopMusic();

                    StreamProxy.SetTrackItem(trackItem);

                    string dataSource = stream ? string.Format("http://127.0.0.1:{0}/{1}", StreamProxy.GetPort(), HttpUtility.UrlEncode(trackItem.FileName, Encoding.UTF8)) : trackItem.FileName;

                    if (stream)
                    {
                        _position = TimeSpan.FromSeconds(trackItem.Child.Duration);
                        ProgressSlider.Minimum = 0;
                        ProgressSlider.Maximum = _position.TotalMilliseconds;
                    }

                    MediaPlayer.Source = new Uri(dataSource);
                    ProgressSlider.Value = 0;
                    _nowPlayingTrack = trackItem;
                    PlayMusic();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Exception:\n\n{0}\n{1}", ex.Message, ex.StackTrace), AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void PlayTrack(TrackItem trackItem)
        {
            UpdateAlbumArt(trackItem.Child);
            QueueTrack(trackItem);
        }
    }
}
