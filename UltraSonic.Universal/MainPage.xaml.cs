using Windows.UI.Xaml.Controls;
using SplitViewMenu;
using UltraSonic.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UltraSonic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var mainViewModel = new MainViewModel();

            mainViewModel.MenuItems.Add(new SimpleNavMenuItem
            {
                Label = "Home",
                DestinationPage = typeof(Views.AlbumList),
                Symbol = Symbol.Home
            });

            mainViewModel.MenuItems.Add(new SimpleNavMenuItem
            {
                Label = "Settings",
                DestinationPage = typeof(Views.Settings),
                Symbol = Symbol.Setting
            });

            DataContext = mainViewModel;
        }
    }
}
