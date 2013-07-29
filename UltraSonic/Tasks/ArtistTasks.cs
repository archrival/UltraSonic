using Subsonic.Common;
using System.Threading.Tasks;
using UltraSonic.Items;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateArtistsTreeView(Task<Indexes> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Dispatcher.Invoke(() =>
                    {
                        ArtistItems.Clear();

                        foreach (Index index in task.Result.Index)
                        {
                            var artistItem = new ArtistItem { Name = index.Name };

                            foreach (Artist artist in index.Artist)
                                artistItem.Children.Add(new ArtistItem { Name = artist.Name, Artist = artist });

                            ArtistItems.Add(artistItem);
                        }
                    });
                    break;
            }
        }
    }
}
