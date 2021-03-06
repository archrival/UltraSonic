﻿using Subsonic.Client.Models;
using Subsonic.Common.Classes;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void PlayNextTrack()
        {
            Dispatcher.Invoke(() =>
            {
                var playNextTrack = false;
                var playlistTrack = _nowPlayingTrack != null ? _playbackTrackItems.IndexOf(_nowPlayingTrack) : PlaybackTrackGrid.SelectedIndex;

                if (playlistTrack == _playbackTrackItems.Count - 1)
                {
                    if (_repeatPlaylist)
                    {
                        if (_playbackFollowsCursor)
                            PlaybackTrackGrid.SelectedIndex = 0;

                        playNextTrack = true;
                    }
                }
                else
                {
                    if (_playbackFollowsCursor)
                        PlaybackTrackGrid.SelectedIndex = playlistTrack + 1;

                    playNextTrack = true;
                }

                StopMusic();

                if (playNextTrack)
                    PlayTrack(_playbackTrackItems[playlistTrack + 1]);
            });
        }

        private void PlayPreviousTrack()
        {
            Dispatcher.Invoke(() =>
            {
                var playlistTrack = _nowPlayingTrack != null ? _playbackTrackItems.IndexOf(_nowPlayingTrack) : PlaybackTrackGrid.SelectedIndex;

                if (_playbackFollowsCursor)
                    PlaybackTrackGrid.SelectedIndex--;

                StopMusic();

                PlayTrack(_playbackTrackItems[playlistTrack - 1]);
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
                MusicTimeRemainingLabel.Content = $"{TimeSpan.FromMilliseconds(MediaPlayer.Position.TotalMilliseconds):mm\\:ss} / {TimeSpan.FromMilliseconds(_position.TotalMilliseconds):mm\\:ss}";
                UpdateTitle();

            }

            MusicPlayStatusLabel.Content = "Stopped";
        }

        private void QueueTrack(TrackModel trackItem)
        {
            if (_streamItems == null) return;

            var child = trackItem.Child;             
            var fileNameUri = new Uri(trackItem.FileName);

            if (_streamItems.All(s => s.OriginalString == trackItem.FileName) && trackItem.Cached)
            {
                // Use this to tell Subsonic we're playing back the track, this will result in the server indicating we have cancelled the data transfer, it isn't very nice.
                //SubsonicClient.StreamAsync(child.Id, trackItem.FileName, _maxBitrate == 0 ? null : (int?) _maxBitrate, null, null, null, null, GetCancellationToken("QueueTrack"), true);
                QueueTrackItemForPlayback(trackItem, false);

                if (_showAlbumArt)
                    UpdateAlbumArt(child);
            }
            else
            {
                if (!_streamItems.Contains(fileNameUri))
                    _streamItems.Enqueue(fileNameUri);

                if (_showAlbumArt)
                    UpdateAlbumArt(child);

                _caching = true;
                DownloadStatusLabel.Content = $"Caching: {child.Title}";
                var streamTask = SubsonicClient.StreamAsync(child.Id, trackItem.FileName, _streamParameters, null, null, null, null, GetCancellationToken("QueueTrack"));
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

        private void QueueTrackItemForPlayback(TrackModel trackItem, bool stream)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    StopMusic();

                    StreamProxy.SetTrackItem(trackItem);

                    string dataSource = stream ? $"http://127.0.0.1:{StreamProxy.GetPort()}/{HttpUtility.UrlEncode(trackItem.FileName, Encoding.UTF8)}" : trackItem.FileName;

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
                    MessageBox.Show($"Exception:\n\n{ex.Message}\n{ex.StackTrace}", AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void PlayTrack(TrackModel trackItem)
        {
            if (_showAlbumArt)
                UpdateAlbumArt(trackItem.Child);

            QueueTrack(trackItem);
        }
    }
}
