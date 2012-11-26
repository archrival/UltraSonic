using System.Windows;
using System.Windows.Input;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for AlbumArt.xaml
    /// </summary>
    public partial class AlbumArt : Window
    {
        public AlbumArt()
        {
            InitializeComponent();
        }

        private void PopupAlbumArtImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(Close);
            MainWindow owner = this.Owner as MainWindow;
            if (owner != null)
                owner.AlbumArtWindow = null;
        }
    }
}
