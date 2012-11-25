using System;

namespace UltraSonic
{
    public class TrackItem : ChildItem
    {
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
        public Guid PlaylistGuid { get; set; }
    }
}