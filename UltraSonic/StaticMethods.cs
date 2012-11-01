using System.Windows.Forms;
using Subsonic.Rest.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private static IEnumerable<TrackItem> GetTrackItemCollection(IEnumerable<Child> children)
        {
            var trackItems = new ObservableCollection<TrackItem>();

            foreach (Child child in children.Where(child => child.IsDir == false && child.Type == MediaType.Music))
                trackItems.Add(new TrackItem
                    {
                        Track = child,
                        Artist = child.Artist,
                        Duration = TimeSpan.FromSeconds(child.Duration),
                        Genre = child.Genre,
                        Title = child.Title,
                        Album = child.Album,
                        TrackNumber = child.Track,
                        DiscNumber = child.DiscNumber,
                        Year = child.Year,
                        Starred = child.Starred != new DateTime(),
                        Rating = child.UserRating
                    });

            return trackItems;
        }

        private static IEnumerable<TrackItem> GetTrackItemCollection(Directory directory)
        {
            return GetTrackItemCollection(directory.Child);
        }

        private static string FileDownloadDialog()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

            DialogResult result = folderDialog.ShowDialog();

            return result.ToString() == "OK" ? folderDialog.SelectedPath : null;
        }
    }
}
