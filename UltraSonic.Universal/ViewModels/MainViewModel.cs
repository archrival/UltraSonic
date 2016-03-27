using SplitViewMenu;
using Subsonic.Client.Tasks;
using System;
using System.Collections.ObjectModel;
using UltraSonic.Views;

namespace UltraSonic
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            MenuItems = new ObservableCollection<SimpleNavMenuItem>();
            InitialPage = typeof(AlbumList);
        }

        public ObservableCollection<SimpleNavMenuItem> MenuItems { get; }
        public Type InitialPage { get; }
    }
}
