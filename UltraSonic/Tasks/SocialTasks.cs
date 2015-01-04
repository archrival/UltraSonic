﻿using Subsonic.Common.Classes;
using Subsonic.Common.Interfaces;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using UltraSonic.Items;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateNowPlayingAlbumImageArt(Task<IImageFormat<Image>> task, UltraSonicNowPlayingItem nowPlayingItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        Image coverArtImage = task.Result.GetImage();

                        if (coverArtImage == null) return;

                        string localFileName = GetCoverArtFilename(nowPlayingItem.Child);
                        coverArtImage.Save(localFileName);

                        nowPlayingItem.Image = coverArtImage.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * ScalingFactor), (int)(_albumArtSize * ScalingFactor));
                    });
                    break;
            }
        }

        private void UpdateNowPlaying(Task<NowPlaying> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        foreach (NowPlayingEntry entry in task.Result.Entries)
                        {
                            string fileName = GetMusicFilename(entry);

                            UltraSonicNowPlayingItem nowPlayingItem = new UltraSonicNowPlayingItem
                            {
                                BitRate = entry.BitRate,
                                DiscNumber = entry.DiscNumber,
                                Duration = TimeSpan.FromSeconds(entry.Duration),
                                Genre = entry.Genre,
                                TrackNumber = entry.Track,
                                Rating = entry.UserRating,
                                Year = entry.Year,
                                Album = entry.Album,
                                Artist = entry.Artist,
                                Child = entry,
                                Starred = (entry.Starred != default(DateTime)),
                                Title = entry.Title,
                                User = entry.Username,
                                AlbumArtSize = _albumArtSize,
                                Cached = IsTrackCached(fileName, entry),
                                FileName = fileName,
                                When = (DateTime.Now - TimeSpan.FromMinutes(entry.MinutesAgo)).ToShortTimeString()
                            };

                            if (_nowPlayingItems.Any(a => a.Album == nowPlayingItem.Album && a.Artist == nowPlayingItem.Artist && a.Starred == nowPlayingItem.Starred && a.Title == nowPlayingItem.Title)) continue;

                            //NowPlayingStatusLabel.Content = string.Format("{0} is playing {1} by {2}", nowPlayingItem.User, nowPlayingItem.Title, nowPlayingItem.Artist);
                            _nowPlayingItems.Add(nowPlayingItem);

                            if (!_showAlbumArt) continue;

                            string localFileName = GetCoverArtFilename(entry);

                            if (File.Exists(localFileName))
                            {
                                Image thisImage = Image.FromFile(localFileName);
                                nowPlayingItem.Image = thisImage.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int) (_albumArtSize * ScalingFactor), (int) (_albumArtSize * ScalingFactor));
                                thisImage.Dispose();
                            }
                            else
                            {
                                SubsonicClient.GetCoverArtAsync(entry.CoverArt).ContinueWith(t => UpdateNowPlayingAlbumImageArt(t, nowPlayingItem));
                            }
                        }
                    });
                    break;
            }
        }

        //private void UpdateChatMessages(Task<ChatMessages> task)
        //{
        //    switch (task.Status)
        //    {
        //        case TaskStatus.RanToCompletion:
        //            Dispatcher.Invoke(() =>
        //            {
        //                TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
        //                _chatMessageSince = t.TotalMilliseconds;

        //                // Order chat messages by time received
        //                foreach (ChatMessage chatMessage in task.Result.ChatMessage.OrderBy(c => c.Time))
        //                {
        //                    DateTime timeStamp = Subsonic.Client.DateTimeExtensions.DateTimeFromUnixTimestamp(chatMessage.Time).ToLocalTime();

        //                    if (_chatMessages.Any(c => c.TimeStamp == timeStamp && c.Message == chatMessage.Message && c.User == chatMessage.Username)) continue;

        //                    if (!SocialTab.IsSelected && _newChatNotify)
        //                        SocialTab.SetValue(StyleProperty, Resources["FlashingHeader"]);

        //                    _chatMessages.Add(new ChatItem(chatMessage));
        //                }

        //                _newChatNotify = true;
        //            });
        //            break;
        //    }
        //}
    }
}
