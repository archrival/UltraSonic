using System.Windows;
using Subsonic.Rest.Api;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UltraSonic.Static;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateAlbumGrid(Task<Subsonic.Rest.Api.Directory> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    UpdateAlbumGrid(task.Result.Child.Where(child => child.IsDir));
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

        private void UpdateAlbumImageArt(Task<Image> task, AlbumItem albumItem)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        Image coverArtImage = task.Result;

                        if (coverArtImage == null) return;

                        string localFileName = GetCoverArtFilename(albumItem.Child);

                        if (!File.Exists(localFileName))
                            coverArtImage.Save(localFileName);

                        BitmapFrame bitmapFrame = coverArtImage.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, _albumArtSize, _albumArtSize);
                        coverArtImage.Dispose();
                        GC.Collect();
                        albumItem.Image = bitmapFrame;
                        AlbumDataGrid.Items.Refresh();
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

                        albumItem.Image = coverArtImage;
                        AlbumDataGrid.Items.Refresh();
                    });
                    break;
                case TaskStatus.Faulted:
                    DownloadCoverArt(albumItem);
                    break;
            }
        }
    }
}
