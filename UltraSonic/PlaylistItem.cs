using System;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class PlaylistItem
    {
        public string Name { get; set; }
        public int Tracks { get; set; }
        public TimeSpan Duration { get; set; }
        public Playlist Playlist { get; set; }
    }
}
