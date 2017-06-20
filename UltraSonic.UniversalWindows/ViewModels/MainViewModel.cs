using System;
using System.Collections.ObjectModel;
using SplitViewMenu;
using Subsonic.Client.Tasks;
using UltraSonic.Views;

namespace UltraSonic.ViewModels
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
