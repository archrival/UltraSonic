using System.IO;
using System.Security.Cryptography;
using System.Text;
using Subsonic.Rest.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Directory = Subsonic.Rest.Api.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private static bool IsTrackCached(string fileName, Child child)
        {
            FileInfo fi = new FileInfo(fileName);
            return fi.Exists && fi.Length == child.Size;
        }

        private static IEnumerable<TrackItem> GetTrackItemCollection(IEnumerable<Child> children)
        {
            var trackItems = new ObservableCollection<TrackItem>();

            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
                trackItems.Add(new TrackItem
                    {
                        Child = child,
                        Artist = child.Artist,
                        Duration = TimeSpan.FromSeconds(child.Duration),
                        Genre = child.Genre,
                        Title = child.Title,
                        Album = child.Album,
                        TrackNumber = child.Track,
                        DiscNumber = child.DiscNumber,
                        Year = child.Year,
                        BitRate = child.BitRate,
                        Starred = child.Starred != new DateTime(),
                        Rating = child.UserRating
                    });

            return trackItems;
        }

        private static IEnumerable<TrackItem> GetTrackItemCollection(Directory directory)
        {
            return GetTrackItemCollection(directory.Child);
        }
    }
}
