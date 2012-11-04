using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class ArtistItem : INotifyPropertyChanged
    {
        public ArtistItem()
        {
            Children = new ObservableCollection<ArtistItem>();
        }

        public string Name { get; set; }
        public ICollection<ArtistItem> Children { get; set; }
        public Artist Artist { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}