using Subsonic.Client.Tasks;
using System.Collections.Generic;
using UltraSonic.Models;

namespace UltraSonic.ViewModels
{
    public class AlbumListViewModel : ObservableObject
    {
        private NotifyTaskCompletion<List<AlbumModel>> _albums;

        public NotifyTaskCompletion<List<AlbumModel>> AlbumList
        {
            get { return _albums; }
            set
            {
                if (value != _albums)
                {
                    _albums = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
