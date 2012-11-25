using Subsonic.Rest.Api;
using System.ComponentModel;

namespace UltraSonic
{
    public class ChildItem : INotifyPropertyChanged
    {
        public bool Starred { get; set; }
        public Child Child { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}