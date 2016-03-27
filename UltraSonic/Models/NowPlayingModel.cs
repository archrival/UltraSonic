using System.Windows.Media.Imaging;

namespace UltraSonic.Models
{
    public class NowPlayingModel : Subsonic.Client.Models.NowPlayingModel
    {
        private BitmapSource _image;
        
        public BitmapSource Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
    }
}
