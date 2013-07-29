using System;
using Subsonic.Common;

namespace UltraSonic.Items
{
    public class TrackItem : ChildItem
    {
        public bool _cached;

        public bool Selected { get; set; }
        public int DiscNumber { get; set; }
        public int TrackNumber { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }
        public int Year { get; set; }
        public int BitRate { get; set; }
        public int Rating { get; set; }
        public string FileName { get; set; }
        public Guid PlaylistGuid { get; set; }
        public TrackItem Source { get; set; }
        public bool Cached
        {
            get { return _cached; }
            set
            {
                _cached = value;
                OnPropertyChanged("Cached");
            }
        }

        public static TrackItem Create(Child child, string cacheDir)
        {
            string fileName = MainWindow.GetMusicFilename(child, cacheDir);

            return new TrackItem
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
                FileName = fileName,
                Cached = MainWindow.IsTrackCached(fileName, child),
                Starred = child.Starred != new DateTime(),
                Rating = child.UserRating
            };
        }
    }
}