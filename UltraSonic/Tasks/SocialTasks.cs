using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Subsonic.Client.Windows;
using Subsonic.Common;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateNowPlayingAlbumImageArt(Task<IImageFormat<Image>> task, NowPlayingItem nowPlayingItem)
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

                        nowPlayingItem.Image = coverArtImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * 1.5), (int)(_albumArtSize * 1.5));
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
                        foreach (NowPlayingEntry entry in task.Result.Entry)
                        {
                            string fileName = GetMusicFilename(entry);

                            NowPlayingItem nowPlayingItem = new NowPlayingItem
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

                            if (Enumerable.Any<NowPlayingItem>(_nowPlayingItems, a => a.Album == nowPlayingItem.Album && a.Artist == nowPlayingItem.Artist && a.Starred == nowPlayingItem.Starred && a.Title == nowPlayingItem.Title)) continue;

                            //NowPlayingStatusLabel.Content = string.Format("{0} is playing {1} by {2}", nowPlayingItem.User, nowPlayingItem.Title, nowPlayingItem.Artist);
                            _nowPlayingItems.Add(nowPlayingItem);

                            if (!_showAlbumArt) continue;

                            string localFileName = GetCoverArtFilename(entry);

                            if (File.Exists(localFileName))
                            {
                                Image thisImage = Image.FromFile(localFileName);
                                nowPlayingItem.Image = thisImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int) (_albumArtSize*1.5), (int) (_albumArtSize*1.5));
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

        private void UpdateChatMessages(Task<ChatMessages> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                        _chatMessageSince = t.TotalMilliseconds;

                        // Order chat messages by time received
                        foreach (ChatMessage chatMessage in task.Result.ChatMessage.OrderBy(c => c.Time))
                        {
                            DateTime timeStamp = StaticMethods.DateTimeFromUnixTimestamp(chatMessage.Time).ToLocalTime();

                            if (Enumerable.Any<ChatItem>(_chatMessages, c => c.TimeStamp == timeStamp && c.Message == chatMessage.Message && c.User == chatMessage.Username)) continue;

                            if (!SocialTab.IsSelected && _newChatNotify)
                                SocialTab.SetValue(FrameworkElement.StyleProperty, Resources["FlashingHeader"]);

                            _chatMessages.Add(new ChatItem { User = chatMessage.Username, Message = chatMessage.Message, TimeStamp = timeStamp });
                        }

                        _newChatNotify = true;
                    });
                    break;
            }
        }


    }
}
