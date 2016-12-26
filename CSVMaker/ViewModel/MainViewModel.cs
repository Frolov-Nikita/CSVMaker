using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows;
using CSVMaker.Model;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace CSVMaker.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields & Propertyes        
        View.MainWindow mw = new View.MainWindow();
        static string ProfileFileName = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) + @"\Profiles.xml";
        Saver ProfileSaver = new Saver() {FileName = ProfileFileName };
        bool initialized = false;

        /// <summary>
        /// Список файлов содержащих таблицы
        /// </summary>
        public ObservableCollection<DataFile> DataFiles { get; set; } = new ObservableCollection<DataFile>();

        public Visibility TreeTipVisibility { get { return DataFiles.Count > 0 ? Visibility.Hidden : Visibility.Visible; } }

        ObservableCollection<InnerTable> _AllTables = new ObservableCollection<InnerTable>();
        /// <summary>
        /// Список всех таблиц во всех файлах
        /// </summary>
        public ObservableCollection<InnerTable> AllTables
        {
            get {return _AllTables;}
        }

        InnerTable _SelectedElementsTable;
        /// <summary>
        /// Выбранная таблица элементов
        /// </summary>
        public InnerTable SelectedElementsTable
        { get { return _SelectedElementsTable; }
            set { _SelectedElementsTable = value; NotifyPropertyChanged("SelectedElementsTable"); }
        }

        string _SelectedStructureHeader = "";
        /// <summary>
        /// Столбец на листе экземпляров, содержащий ссылку на таблицу структуры
        /// </summary>
        public string SelectedStructureHeader
        {
            get { return _SelectedStructureHeader; }
            set { _SelectedStructureHeader = value; NotifyPropertyChanged("SelectedStructureHeader"); }
        }

        string _SelectedExportFile = "";
        /// <summary>
        /// Выбранный файл для экспорта
        /// </summary>
        public string SelectedExportFile
        {
            get { return _SelectedExportFile; }
            set { _SelectedExportFile = value; NotifyPropertyChanged("SelectedExportFile"); }
        }

        ObservableCollection<string> _LastExportFilesList = new ObservableCollection<string>();
        /// <summary>
        /// Список недавних файлов
        /// </summary>
        public ObservableCollection<string> LastExportFilesList
        {
            get { return _LastExportFilesList; }
            set { _LastExportFilesList = value; NotifyPropertyChanged("LastExportFilesList"); }
        }

        public Profile SelectedProfile { get; set; }
        ObservableCollection<Profile> _Profiles = new ObservableCollection<Profile>();
        /// <summary>
        /// Список профилей экспорта
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get { return _Profiles; }
            set { _Profiles = value; NotifyPropertyChanged("Profiles"); }
        }

        ObservableCollection<string> _Errors = new ObservableCollection<string>();
        /// <summary>
        /// Ошибки
        /// </summary>
        public ObservableCollection<string> Errors { get {return _Errors; } }
        
        #endregion

        #region Комманды
        public CommandRef ExportCSV { get; set; }
        public CommandRef EditProfiles { get; set; }
        public CommandRef SaveFileDialogCSV { get; set; }
        #endregion

        #region Конструктор

        public MainViewModel()
        {
            
            #region Связываем события
            DataFiles.CollectionChanged += DataFiles_CollectionChanged;
            Profiles.CollectionChanged += Profiles_CollectionChanged;
            this.PropertyChanged += SelectedAny_PropertyChanged;
            #endregion

            #region пробуем загрузить старый конфиг
            // Загружаем профили, если они есть
            if (File.Exists(ProfileFileName) && false)
            {
                ProfileSaver.Open(ref _Profiles);
                NotifyPropertyChanged("Profiles");
            }
            else
            {
                Profiles = new ObservableCollection<Profile>();
                Profiles.Add( new Profile() { Name = "NewProfile" } );
                ProfileSaver.Save(_Profiles);
            }


            // Пробуем загрузить настройки последней сессии
            //  загружаем последние файлы
            if(Properties.Settings.Default.DataFileList!=null)
                foreach (string dfn in Properties.Settings.Default.DataFileList)
                    if (File.Exists(dfn))
                        DataFiles.Add(new DataFile(dfn));

            SelectedExportFile = Properties.Settings.Default.SelectedExportFile;
            SelectedStructureHeader = Properties.Settings.Default.SelectedStructureHeader;

            if (Properties.Settings.Default.LastExportFilesList != null)
                foreach (string lfn in Properties.Settings.Default.LastExportFilesList)
                    LastExportFilesList.Add(lfn);

            SelectedElementsTable = (from t in AllTables
                                    where   (t.Name == Properties.Settings.Default.SelectedElement) &&
                                            (t.Parent.Name == Properties.Settings.Default.SelectedElement_df)
                                    select t).FirstOrDefault();            

            SelectedProfile = (from p in Profiles
                                      where (p.Name == Properties.Settings.Default.SelectedProfile)
                                      select p).FirstOrDefault();
            
            #endregion

            #region Инициируем команды
            ExportCSV = new CommandRef((args) => { ExportCSVMethod(); });
            ExportCSV.CanExecuteDelegate = (arg)=>{
                return  (SelectedElementsTable != null) &&
                        (SelectedProfile != null) &&
                        (SelectedStructureHeader != "");
            };

            EditProfiles = new CommandRef((args) => { EditProfilesMethod(); });
            SaveFileDialogCSV = new CommandRef((args) => { SaveFileDialogCSVMethod(); });
            #endregion
            
            mw.DataContext = this;
            mw.Show();
            initialized = true;
        }

        #endregion

        #region Методы
        /// <summary>
        /// Проверка правильности конфигураций
        /// </summary>
        /// <returns>Есть ошибки при проверке</returns>
        bool CheckConfiguration()
        {
            string TmpTableName;
            string LastTableName = "";
            bool Err = false;

            if (SelectedElementsTable.Table.Rows.Count == 0){
                Log("Таблица экземпляров " + SelectedElementsTable.Name + " не содержит экземпляров");
                Err |= true;
            }
            
            foreach(DataRow edr in SelectedElementsTable.Table.Rows){
                TmpTableName = edr[SelectedStructureHeader].ToString();
                // ссылка на структуру пуста
                if (TmpTableName == "") {
                    Log("Не указано имя структуры для элемента: " + edr[0].ToString());
                    Err |= true;
                    continue; }

                // все остальные тесты касаются структуры, если она посвторяется пропускаем
                if (TmpTableName == LastTableName) continue;
                    
                LastTableName = TmpTableName;
                // все используемые структуры существуют
                if (GetTableByName(TmpTableName) == null){
                    Log("Указанная структура для элемента: " + edr[0].ToString() + " отсутствует строка: " + SelectedElementsTable.Table.Rows.IndexOf(edr).ToString());
                    Err |= true;
                    continue;}

                // все используемые структуры имеют все требуемые профилем заголовки
                foreach(string cName in (from c in SelectedProfile.Rules where c.MastExist select c.Name ))
                    if (!GetTableByName(TmpTableName).Headers.Contains(cName)){
                        Log("Cтруктура: " + TmpTableName + " не содержит обязательного заголовка " + cName);
                        Err |= true;
                    }

            }// end_foreach
                

            // ???? все используемые структуры имеют одинаковый заголовок (последовательность не важна)
            return Err;    
        }

        /// <summary>
        /// процедура экспорта
        /// </summary>
        void ExportCSVMethod()
        {
            string tmp="";
            InnerTable strTmp;
            DataTable resTmpTable;

            Errors.Clear();
            if (CheckConfiguration())
            {
                Log("Сборка отменена, из-за ошибок.");
                return;
            }

            if (SelectedExportFile == "")
                if (!SaveFileDialogCSVMethod())
                {
                    Log("Сборка отменена, не казан файл");
                    return;
                }

            // Добавляем заголовок в файл
            if (SelectedProfile.HeaderType == HeaderType.Custom)
                File.WriteAllText(SelectedExportFile, SelectedProfile.CustomHeader);

            if (SelectedProfile.HeaderType == HeaderType.None)
                File.WriteAllText(SelectedExportFile, "");

            if (SelectedProfile.HeaderType == HeaderType.FromStruct){
                strTmp = GetTableByName(SelectedElementsTable.Table.Rows[0][SelectedStructureHeader].ToString());
                tmp = "";
                foreach (DataColumn c in strTmp.Table.Columns) 
                    tmp += c.ColumnName + SelectedProfile.FieldSeparator;                
                File.WriteAllText(SelectedExportFile, tmp.Substring(0, tmp.Length - 1) + "\n",SelectedProfile.Encoding);
            }

            // создаем экспортер
            ExporterCSV ExpCSV = new ExporterCSV(SelectedExportFile, SelectedProfile);
            ExpCSV.RiseLogMessageEvent += Log;

            // перебираем элементы и записываем
            try
            {
                foreach (DataRow eDR in SelectedElementsTable.Table.Rows)
                {
                    strTmp = GetTableByName(eDR[SelectedStructureHeader].ToString());
                    resTmpTable = ExpCSV.SubstituteStructureCopy(eDR, strTmp.Table);

                    //Проверяем на правила и записываем в файл
                    File.AppendAllLines(SelectedExportFile, ExpCSV.MakeStrings(resTmpTable), SelectedProfile.Encoding);                    
                }
                Log("Сборка завершена успешно");
            }
            catch(Exception ex)
            { Log("Сборка прервана из-за ошибки: " + ex.Message); }
            
        }//void ExportCSVMethod()
        
        /// <summary>
        /// Поиск таблицы по имени
        /// </summary>
        /// <param name="Name">имя таблицы</param>
        /// <returns></returns>
        InnerTable GetTableByName(string Name)
        {
            foreach (var t in SelectedElementsTable.Parent.Tables)
                if (t.SearchName == Name) return t;

            foreach (var t in _AllTables)
                if (t.SearchName == Name) return t;

            return null;
        }

        /// <summary>
        /// Записать в лог
        /// </summary>
        /// <param name="message">сообщение</param>
        void Log(string message){
            Errors.Add(message);
            if(mw.BottomRow.Height.Value<20)
                mw.BottomRow.Height = new GridLength(100);
        }

        bool SaveFileDialogCSVMethod()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "CSV Files|*.csv",
                Title = "Как именно сохранить?",
            };
            if (sfd.ShowDialog() == true)
            {
                SelectedExportFile = sfd.FileName;
                if(!LastExportFilesList.Contains(sfd.FileName))
                    LastExportFilesList.Add(sfd.FileName);
                while (LastExportFilesList.Count > 15) LastExportFilesList.RemoveAt(0);
                NotifyPropertyChanged("LastExportFilesList");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Открываем редактор профилей
        /// </summary>
        private void EditProfilesMethod()
        {
            ViewModel.ProfileEditorViewModel PEVM = new ViewModel.ProfileEditorViewModel(this);
        }
        #endregion

        #region обработчики событий
        // Изменение коллекции файлов
        private void DataFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var ni in e.NewItems) _AllTables.AddRange(((DataFile)ni).Tables);
            if (e.OldItems != null)
                foreach (var oi in e.OldItems) _AllTables.RemoveRange(((DataFile)oi).Tables);

            NotifyPropertyChanged("AllTables");
            NotifyPropertyChanged("TreeTipVisibility");

        }

        private void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ProfileSaver.Save(_Profiles);
        }

        private void SelectedAny_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!initialized) return;

            Properties.Settings.Default.SelectedElement = SelectedElementsTable == null ? "" : SelectedElementsTable.Name;
            Properties.Settings.Default.SelectedElement_df = SelectedElementsTable == null ? "" : SelectedElementsTable.Parent.Name;
            
            Properties.Settings.Default.SelectedProfile = SelectedProfile == null ? "" : SelectedProfile.Name;
            Properties.Settings.Default.SelectedStructureHeader = SelectedStructureHeader;
            Properties.Settings.Default.SelectedExportFile = SelectedExportFile;

            Properties.Settings.Default.DataFileList = new System.Collections.Specialized.StringCollection();
            foreach (DataFile df in DataFiles) Properties.Settings.Default.DataFileList.Add(df.Name);

            Properties.Settings.Default.LastExportFilesList = new System.Collections.Specialized.StringCollection();
            foreach (var lef in LastExportFilesList) Properties.Settings.Default.LastExportFilesList.Add(lef);

            if (e.PropertyName == "Profiles")
                ProfileSaver.Save(_Profiles);

            Properties.Settings.Default.Save();
        }
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
    }//MainViewModel
    
}
