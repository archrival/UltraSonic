using System.ComponentModel;
using System.Windows.Media.Imaging;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class AlbumItem : INotifyPropertyChanged
    {
        public bool Starred { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public Child Album { get; set; }
        public BitmapSource Image { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}