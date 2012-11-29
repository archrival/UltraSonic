using Subsonic.Rest.Api;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateNowPlayingAlbumImageArt(Task<Image> task, NowPlayingItem nowPlayingItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        Image coverArtImage = task.Result;

                        if (coverArtImage == null) return;

                        string localFileName = GetCoverArtFilename(nowPlayingItem.Child);
                        coverArtImage.Save(localFileName);

                        nowPlayingItem.Image = coverArtImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * 1.5), (int)(_albumArtSize * 1.5));
                        NowPlayingDataGrid.Items.Refresh();
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
                                When = (DateTime.Now - TimeSpan.FromMinutes(entry.MinutesAgo)).ToShortTimeString()
                            };

                            if (_nowPlayingItems.Any(a => a.Album == nowPlayingItem.Album && a.Artist == nowPlayingItem.Artist && a.Starred == nowPlayingItem.Starred && a.Title == nowPlayingItem.Title)) continue;

                            _nowPlayingItems.Add(nowPlayingItem);
                            string localFileName = GetCoverArtFilename(entry);

                            if (File.Exists(localFileName))
                            {
                                Image thisImage = Image.FromFile(localFileName);
                                nowPlayingItem.Image = thisImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * 1.5), (int)(_albumArtSize * 1.5));
                                thisImage.Dispose();
                                NowPlayingDataGrid.Items.Refresh();
                            }
                            else
                            {
                                SubsonicApi.GetCoverArtAsync(entry.CoverArt).ContinueWith(t => UpdateNowPlayingAlbumImageArt(t, nowPlayingItem));
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

                            if (_chatMessages.Any(c => c.TimeStamp == timeStamp && c.Message == chatMessage.Message && c.User == chatMessage.Username)) continue;

                            if (!SocialTab.IsSelected && _newChatNotify)
                                SocialTab.SetValue(StyleProperty, Resources["FlashingHeader"]);

                            _chatMessages.Add(new ChatItem { User = chatMessage.Username, Message = chatMessage.Message, TimeStamp = timeStamp });
                        }

                        _newChatNotify = true;
                    });
                    break;
            }
        }


    }
}
