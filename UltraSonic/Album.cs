using Subsonic.Common.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using UltraSonic.Models;
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
                    MusicCoverArt.Source = _currentAlbumArt.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int)(MusicCoverArt.Width * ScalingFactor), (int)(MusicCoverArt.Height * ScalingFactor));
                }
                else
                {
                    SubsonicClient.GetCoverArtAsync(child.CoverArt, null, GetCancellationToken("UpdateAlbumArt")).ContinueWith(t => UpdateCoverArt(t, child));
                }
            });
        }

        private void UpdateAlbumGrid(IEnumerable<Child> children)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      ProgressIndicator.Visibility = System.Windows.Visibility.Visible;

                                      _albumItems.Clear();

                                      var view = CollectionViewSource.GetDefaultView(AlbumDataGrid.ItemsSource);
                                      
                                      if (view != null && view.SortDescriptions.Count > 0)
                                          view.SortDescriptions.Clear();

                                      foreach (var column in AlbumDataGrid.Columns)
                                      {
                                          column.SortDirection = null;
                                          column.Width = column.MinWidth;
                                          column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                                      }

                                      var enumerable = children as IList<Child> ?? children.ToList();

                                      SemaphoreSlim throttler = null;

                                      if (_throttle > 0)
                                          throttler = new SemaphoreSlim(enumerable.Count < _throttle ? enumerable.Count : _throttle);

                                      foreach (
                                          var albumItem in
                                              enumerable.Select(
                                                  child =>
                                                  new AlbumModel
                                                      {
                                                          AlbumArtSize = _albumArtSize,
                                                          Artist = child.Artist,
                                                          Name = child.Album,
                                                          Rating = child.UserRating,
                                                          Child = child,
                                                          Year = child.Year,
                                                          Parent = child.Parent,
                                                          Starred = (child.Starred != default(DateTime))
                                                      }).Where(a => a.Child.IsDir))
                                      {
                                          _albumItems.Add(albumItem);

                                          if (_showAlbumArt)
                                              AlbumDataGridAlbumArtColumn.Visibility = System.Windows.Visibility.Visible;
                                          else
                                          {
                                              AlbumDataGridAlbumArtColumn.Visibility =
                                                  System.Windows.Visibility.Collapsed;
                                              continue;
                                          }

                                          throttler?.WaitAsync();

                                          try
                                          {
                                              AlbumModel item = albumItem;

                                              Task.Run(async () =>
                                                                 {
                                                                     try
                                                                     {
                                                                         await Task.Delay(1);
                                                                         var image = Image.FromFile(GetCoverArtFilename(item.Child)); 
                                                                         var bitmapFrame = image.ToBitmapSource().Resize(BitmapScalingMode.HighQuality, true, (int)(_albumArtSize * ScalingFactor), (int)(_albumArtSize * ScalingFactor));
                                                                         image.Dispose();
                                                                         bitmapFrame.Freeze();
                                                                         GC.Collect();
                                                                         return bitmapFrame;
                                                                     }
                                                                     finally
                                                                     {
                                                                         throttler?.Release();
                                                                     }
                                                                 }).ContinueWith(t => UpdateAlbumImageArt(t, item));
                                          }
                                          catch
                                          {
                                              DownloadCoverArt(albumItem);
                                          }
                                      }

                                      UiHelpers.ScrollToTop(AlbumDataGrid);
                                      ProgressIndicator.Visibility = System.Windows.Visibility.Hidden;
                                  });
        }
    }
}
