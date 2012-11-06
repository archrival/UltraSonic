using System;
using System.ComponentModel;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public sealed class TrackItem : INotifyPropertyChanged
    {
        public bool Selected { get; set; }
        public int DiscNumber { get; set; }
        public int TrackNumber { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public TimeSpan Duration { get; set; }
        public int Year { get; set; }
        public int BitRate { get; set; }
        public int Rating { get; set; }
        public bool Starred { get; set; }
        public Child Track { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}