using System.Runtime.CompilerServices;
using Subsonic.Rest.Api;
using System.ComponentModel;
using UltraSonic.Annotations;

namespace UltraSonic
{
    public class ChildItem : INotifyPropertyChanged
    {
        public bool Starred { get; set; }
        public Child Child { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}