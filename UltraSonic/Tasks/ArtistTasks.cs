﻿using Subsonic.Client.Models;
using Subsonic.Common.Classes;
using System.Threading.Tasks;

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

                        if (task.Result == null || task.Result.Items == null)
                            return;

                        foreach (Index index in task.Result.Items)
                        {
                            var artistItem = new ArtistModel { Name = index.Name };

                            foreach (Artist artist in index.Artists)
                                artistItem.Children.Add(new ArtistModel { Name = artist.Name, Artist = artist });

                            ArtistItems.Add(artistItem);
                        }
                    });
                    break;
            }
        }
    }
}
