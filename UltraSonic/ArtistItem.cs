using System.Collections.Generic;
using System.Collections.ObjectModel;
using Subsonic.Rest.Api;

namespace UltraSonic
{
    public class ArtistItem
    {
        public ArtistItem()
        {
            Children = new ObservableCollection<ArtistItem>();
        }

        public string Name { get; set; }
        public ICollection<ArtistItem> Children { get; set; }
        public Artist Artist { get; set; }
    }
}