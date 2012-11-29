using System.Windows.Input;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for AlbumArt.xaml
    /// </summary>
    public partial class AlbumArt
    {
        public AlbumArt()
        {
            InitializeComponent();
        }

        private void PopupAlbumArtImageMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseAlbumArtWindow();
        }

        private void WindowCloseOnKeyDown(object sender, KeyEventArgs e)
        {
            CloseAlbumArtWindow();
        }

        private void CloseAlbumArtWindow()
        {
            Dispatcher.Invoke(Close);

            MainWindow owner = Owner as MainWindow;

            if (owner != null)
                owner.AlbumArtWindow = null;
        }
    }
}
