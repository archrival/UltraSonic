using Subsonic.Common;
using System.ComponentModel;

namespace UltraSonic.Items
{
    public sealed class AlbumListItem : INotifyPropertyChanged
    {
        public AlbumListType Type { get; set; }
        public int Current { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}