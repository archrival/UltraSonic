using Subsonic.Client.Tasks;
using Subsonic.Common.Enums;
using Subsonic.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltraSonic.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UltraSonic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumList : Page
    {
        public AlbumListViewModel AlbumListViewModel;

        public AlbumList()
        {
            var albumListViewModel = new AlbumListViewModel();

            AlbumListViewModel = albumListViewModel;
            DataContext = albumListViewModel;

            InitializeComponent();

            AlbumListViewModel.AlbumList = new NotifyTaskCompletion<List<Models.AlbumModel>>(GetAlbumList(AlbumListType.AlphabeticalByName, SettingsHelper.GetMaxAlbumResults()));
        }

        private async Task<List<Models.AlbumModel>> GetAlbumList(AlbumListType albumListType, int size)
        {
            var subsonicClient = SettingsHelper.GetSubsonicClient();

            if (subsonicClient == null)
                return null;

            var albumList = await subsonicClient.GetAlbumListAsync(albumListType, size);

            var albumModels = new List<Models.AlbumModel>();

            foreach (var album in albumList.Albums)
            {
                var albumModel = new Models.AlbumModel
                {
                    Artist = album.Artist,
                    Child = album,
                    CoverArt = album.CoverArt,
                    Genre = album.Genre,
                    Id = album.Id,
                    Name = album.Title,
                    Parent = album.Parent,
                    Rating = album.UserRating,
                    Starred = album.Starred != default(DateTime),
                    Year = album.Year,
                    Image = await GetCoverArt(album.CoverArt ?? album.Id, 200)
                };

                albumModels.Add(albumModel);
            }

            return albumModels;
        }

        private async Task<IImageFormat<SoftwareBitmapSource>> GetCoverArt(string id, int size)
        {
            var subsonicClient = SettingsHelper.GetSubsonicClient();

            if (subsonicClient == null)
                return null;

            return await subsonicClient.GetCoverArtAsync(id, size);
        }
    }
}
