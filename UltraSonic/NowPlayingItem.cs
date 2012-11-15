using Subsonic.Rest.Api;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace UltraSonic
{
    public sealed class NowPlayingItem : INotifyPropertyChanged 
    {
        public bool Starred { get; set; }
        public string User { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string When { get; set; }
        public Child Entry { get; set; }
        public BitmapSource Image { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
