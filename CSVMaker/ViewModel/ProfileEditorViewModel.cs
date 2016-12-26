using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSVMaker.Model;

namespace CSVMaker.ViewModel
{
    public class ProfileEditorViewModel : INotifyPropertyChanged
    {
        #region Fields & Propertyes
        MainViewModel mvm;
        View.ProfileEditor pew;
        public ObservableCollection<Profile> Profiles { get; set; }

        public ObservableCollection<Profile> ProfilesCpy { get; set; }

        Profile _SelectedProfile;
        public Profile SelectedProfile { get { return _SelectedProfile; }
            set { _SelectedProfile = value; NotifyPropertyChanged("SelectedProfile"); } }
        #endregion

        #region Комманды
        public CommandRef Save { get; set; }
        public CommandRef Cancel { get; set; }
        #endregion

        #region Конструктор
        public ProfileEditorViewModel(MainViewModel mvm)
        {//ObservableCollection<Profile> profiles
            this.mvm = mvm;
            ProfilesCpy = new ObservableCollection<Profile>(mvm.Profiles);

            #region Инициируем команды
            Save = new CommandRef((args)=> { SaveMethod(); });
            Cancel = new CommandRef((args) => { CancelMethod(); });
            #endregion

            pew = new View.ProfileEditor();
            pew.DataContext = this;
            pew.Show();
            pew.DGProfList.SelectedIndex = 0;
        }
        #endregion

        #region методы
        void SaveMethod()
        {
            mvm.Profiles = ProfilesCpy;
            pew.Close();
        }
        void CancelMethod()
        {
            pew.Close();
        }
        #endregion

        #region обработчики событий
        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
