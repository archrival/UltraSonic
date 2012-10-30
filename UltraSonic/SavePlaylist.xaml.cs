using System.Windows;

namespace UltraSonic
{
    /// <summary>
    /// Interaction logic for SavePlaylist.xaml
    /// </summary>
    public partial class SavePlaylist : Window
    {
        public string PlaylistName { get; set; }

        public SavePlaylist()
        {
            InitializeComponent();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            PlaylistName = PlaylistNameTextBox.Text;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
