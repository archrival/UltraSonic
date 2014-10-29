using System.Windows.Media.Imaging;
using Subsonic.Client.Items;

namespace UltraSonic.Items
{
    public class UltraSonicNowPlayingItem : NowPlayingItem
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
