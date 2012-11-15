using System.Windows.Media.Imaging;

namespace UltraSonic
{
    public sealed class NowPlayingItem : TrackItem
    {
        public string User { get; set; }
        public string When { get; set; }
        public BitmapSource Image { get; set; }
    }
}
