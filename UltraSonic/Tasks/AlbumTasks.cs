using Subsonic.Client.Items;
using Subsonic.Common.Classes;
using Subsonic.Common.Interfaces;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UltraSonic.Static;
using Directory = Subsonic.Common.Classes.Directory;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateAlbumGrid(Task<Directory> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    UpdateAlbumGrid(task.Result.Child.Where(child => child.IsDir));
                    UpdateTrackListingGrid(task.Result.Child.Where(child => !child.IsDir));
                    break;
            }
        }

        private void UpdateAlbumGrid(Task<AlbumList> task, int albumListStart, int albumListEnd)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    if (task.Result.Album.Any())
                    {
                        Dispatcher.Invoke(() =>
                                              {
                                                  AlbumDataGridNext.Header = string.Format("Albums {0} - {1}", albumListStart, albumListEnd);
                                                  AlbumDataGridNext.Visibility = Visibility.Visible;
                                              });
                    }
                    else
                    {
                        Dispatcher.Invoke(() => { AlbumDataGridNext.Visibility = Visibility.Collapsed; });
                    }

                    UpdateAlbumGrid(task.Result.Album);
                    break;
            }
        }

        private void UpdateAlbumImageArt(Task<IImageFormat<Image>> task, AlbumItem albumItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        if (task.Result == null)
                            return;

                        Image coverArtImage = task.Result.GetImage();

                        if (coverArtImage == null) return;

                        string localFileName = GetCoverArtFilename(albumItem.Child);

                        if (!File.Exists(localFileName))
                            coverArtImage.Save(localFileName);

                        BitmapFrame bitmapFrame = coverArtImage.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * ScalingFactor), (int)(_albumArtSize * ScalingFactor));
                        coverArtImage.Dispose();
                        GC.Collect();
                        //TODO: albumItem.Image = bitmapFrame;
                    });
                    break;
            }
        }

        private void UpdateAlbumImageArt(Task<BitmapFrame> task, AlbumItem albumItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        BitmapFrame coverArtImage = task.Result;
                        if (coverArtImage == null) return;
                        //TODO: albumItem.Image = coverArtImage;
                    });
                    break;
                case TaskStatus.Faulted:
                    DownloadCoverArt(albumItem);
                    break;
            }
        }
    }
}
