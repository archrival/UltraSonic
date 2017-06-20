using Subsonic.Client.Tasks;
using System.Windows.Input;
using UltraSonic.Models;

namespace UltraSonic.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private static SettingsModel _settings;
        private ICommand _saveSettingsCommand;

        public SettingsViewModel()
        {
            Settings = SettingsHelper.GetSettings();
        }

        public SettingsModel Settings
        {
            get { return _settings; }
            set
            {
                if (value != _settings)
                {
                    _settings = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveSettingsCommand
        {
            get
            {
                if (_saveSettingsCommand == null)
                {
                    _saveSettingsCommand = new RelayCommand(
                        param => SaveSettings(),
                        param => (Settings != null)
                    );
                }
                return _saveSettingsCommand;
            }
        }

        private void SaveSettings()
        {
            SettingsHelper.SaveSettings(this);
        }
    }
}
