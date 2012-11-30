using System.Windows.Controls;
using Subsonic.Rest.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UltraSonic.Static;
using Image = System.Drawing.Image;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateAlbumArt(Child child)
        {
            Dispatcher.Invoke(() =>
            {
                string localFileName = GetCoverArtFilename(child);
                if (File.Exists(localFileName))
                {
                    _currentAlbumArt = Image.FromFile(localFileName);
                    MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int)MusicCoverArt.Width, (int)MusicCoverArt.Height);
                }
                else
                {
                    SubsonicApi.GetCoverArtAsync(child.CoverArt, null, GetCancellationToken("UpdateAlbumArt")).ContinueWith(t => UpdateCoverArt(t, child));
                }
            });
        }

        private void UpdateAlbumGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() =>
            {
                _albumItems.Clear();

                foreach (DataGridColumn column in AlbumDataGrid.Columns)
                {
                    column.Width = column.MinWidth;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                var enumerable = children as IList<Child> ?? children.ToList();

                SemaphoreSlim throttler = null;

                if (_throttle > 0)
                     throttler = new SemaphoreSlim(enumerable.Count < _throttle ? enumerable.Count : _throttle);

                foreach (AlbumItem albumItem in enumerable.Select(child => new AlbumItem { AlbumArtSize = _albumArtSize, Artist = child.Artist, Name = child.Album, Child = child, Starred = (child.Starred != default(DateTime)) }))
                {
                    _albumItems.Add(albumItem);

                    if (_showAlbumArt)
                        AlbumDataGridAlbumArtColumn.Visibility = System.Windows.Visibility.Visible;
                    else
                    {
                        AlbumDataGridAlbumArtColumn.Visibility = System.Windows.Visibility.Collapsed;
                        continue;
                    }

                    if (throttler != null) throttler.WaitAsync();

                    try
                    {
                        AlbumItem item = albumItem;

                        Task.Run(async () =>
                                           {
                                               try
                                               {
                                                   await Task.Delay(1);
                                                   Image image = Image.FromFile(GetCoverArtFilename(item.Child));
                                                   BitmapFrame bitmapFrame = image.ToBitmapSource().Resize(System.Windows.Media.BitmapScalingMode.HighQuality, true, (int) (_albumArtSize*1.5), (int) (_albumArtSize*1.5));
                                                   image.Dispose();
                                                   bitmapFrame.Freeze();
                                                   GC.Collect();
                                                   return bitmapFrame;
                                               }
                                               finally
                                               {
                                                   if (throttler != null) throttler.Release();
                                               }
                                           }).ContinueWith(t => UpdateAlbumImageArt(t, item));
                    }
                    catch
                    {
                        DownloadCoverArt(albumItem);
                    }
                }

                UiHelpers.ScrollToTop(AlbumDataGrid);
            });
        }

    }
}
