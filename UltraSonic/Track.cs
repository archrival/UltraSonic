using Subsonic.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UltraSonic.Items;
using Directory = Subsonic.Common.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        public static bool IsTrackCached(string fileName, Child child)
        {
            var fi = new FileInfo(fileName);
            return fi.Exists && fi.Length == child.Size;
        }

        private IEnumerable<TrackItem> GetTrackItemCollection(IEnumerable<Child> children)
        {
            var trackItems = new ObservableCollection<TrackItem>();

            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
                trackItems.Add(TrackItem.Create(child, _musicCacheDirectoryName));

            return trackItems;
        }

        private void PopulateTrackItemCollection(IEnumerable<Child> children)
        {
            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
                _trackItems.Add(TrackItem.Create(child, _musicCacheDirectoryName));
        }

        private IEnumerable<TrackItem> GetTrackItemCollection(Directory directory)
        {
            return GetTrackItemCollection(directory.Child);
        }
    }
}