using Subsonic.Client;
using Subsonic.Client.Items;
using Subsonic.Common.Classes;
using Subsonic.Common.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Directory = Subsonic.Common.Classes.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private static bool IsTrackCached(string fileName, Child child)
        {
            var fi = new FileInfo(fileName);
            return fi.Exists && fi.Length == child.Size;
        }

        private IEnumerable<TrackItem> GetTrackItemCollection(IEnumerable<Child> children)
        {
            var trackItems = new ObservableCollection<TrackItem>();

            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
            {
                string fileName = GetMusicFilename(child, _musicCacheDirectoryName);
                bool isCached = IsTrackCached(fileName, child);
                trackItems.Add(TrackItem.Create(child, fileName, isCached));
            }

            return trackItems;
        }

        private void PopulateTrackItemCollection(IEnumerable<Child> children)
        {
            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
            {
                string fileName = GetMusicFilename(child, _musicCacheDirectoryName);
                bool isCached = IsTrackCached(fileName, child);
                _trackItems.Add(TrackItem.Create(child, fileName, isCached));
            }
        }

        private IEnumerable<TrackItem> GetTrackItemCollection(Directory directory)
        {
            return GetTrackItemCollection(directory.Children);
        }
    }
}