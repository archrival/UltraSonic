using System;
using System.ComponentModel;
using System.Threading;
using Subsonic.Rest.Api;
using System.Threading.Tasks;

namespace UltraSonic
{
    public class DownloadItem : INotifyPropertyChanged 
    {
        public Task<long> Task { get; set; }
        public Child Child { get; set; }
        public string Path { get; set; }
        public bool IsComplete { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
