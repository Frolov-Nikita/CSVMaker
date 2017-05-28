using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSVMaker.Model;

namespace CSVMaker.ViewModel
{
    public class ProfileEditorViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mvm;
        private readonly View.ProfileEditor _pew;
        public ObservableCollection<Profile> Profiles { get; set; }

        public ObservableCollection<Profile> ProfilesCpy { get; set; }

        private Profile _selectedProfile;
        public Profile SelectedProfile { get => _selectedProfile;
            set { _selectedProfile = value; NotifyPropertyChanged(); } }
        
        public CommandRef Save { get; set; }
        public CommandRef Cancel { get; set; }
        
        public ProfileEditorViewModel(MainViewModel mvm)
        {//ObservableCollection<Profile> profiles
            _mvm = mvm;
            ProfilesCpy = new ObservableCollection<Profile>(mvm.Profiles);
            
            Save = new CommandRef((args)=> { SaveMethod(); });
            Cancel = new CommandRef((args) => { CancelMethod(); });

            _pew = new View.ProfileEditor {DataContext = this};
            _pew.Show();
            _pew.DGProfList.SelectedIndex = 0;
        }

        private void SaveMethod()
        {
            _mvm.Profiles = ProfilesCpy;
            OnDone?.Invoke();
            _pew.Close();
        }

        private void CancelMethod()
        {
            _pew.Close();
        }

        public delegate void Ui();
        public event Ui OnDone;
        
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Реализация интерфейса INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
