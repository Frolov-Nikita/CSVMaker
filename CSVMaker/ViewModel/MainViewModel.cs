using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using CSVMaker.Model;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;

namespace CSVMaker.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly View.MainWindow _mw = new View.MainWindow();
        private static readonly string ProfileFileName = /*Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) +*/ @"Profiles.xml";
        private readonly Saver _profileSaver = new Saver() {FileName = ProfileFileName };
        private readonly bool _initialized;

        /// <summary>
        /// Список файлов содержащих таблицы
        /// </summary>
        public ObservableCollection<DataFile> DataFiles { get; set; } = new ObservableCollection<DataFile>();

        public Visibility TreeTipVisibility => DataFiles.Count > 0 ? Visibility.Hidden : Visibility.Visible;

        private readonly ObservableCollection<InnerTable> _allTables = new ObservableCollection<InnerTable>();
        /// <summary>
        /// Список всех таблиц во всех файлах
        /// </summary>
        public ObservableCollection<InnerTable> AllTables => _allTables;

        private InnerTable _selectedElementsTable;
        /// <summary>
        /// Выбранная таблица элементов
        /// </summary>
        public InnerTable SelectedElementsTable
        { get { return _selectedElementsTable; }
            set { _selectedElementsTable = value; NotifyPropertyChanged(); }
        }

        private string _selectedStructureHeader = "";
        /// <summary>
        /// Столбец на листе экземпляров, содержащий ссылку на таблицу структуры
        /// </summary>
        public string SelectedStructureHeader
        {
            get { return _selectedStructureHeader; }
            set { _selectedStructureHeader = value; NotifyPropertyChanged(); }
        }

        private string _selectedExportFile = "";
        /// <summary>
        /// Выбранный файл для экспорта
        /// </summary>
        public string SelectedExportFile
        {
            get { return _selectedExportFile; }
            set { _selectedExportFile = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<string> _lastExportFilesList = new ObservableCollection<string>();
        /// <summary>
        /// Список недавних файлов
        /// </summary>
        public ObservableCollection<string> LastExportFilesList
        {
            get { return _lastExportFilesList; }
            set { _lastExportFilesList = value; NotifyPropertyChanged(); }
        }

        public Profile SelectedProfile { get; set; }
        private ObservableCollection<Profile> _profiles = new ObservableCollection<Profile>();
        /// <summary>
        /// Список профилей экспорта
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get { return _profiles; }
            set { _profiles = value; NotifyPropertyChanged(); }
        }

        private readonly ObservableCollection<string> _errors = new ObservableCollection<string>();
        /// <summary>
        /// Ошибки
        /// </summary>
        public ObservableCollection<string> Errors { get {return _errors; } }
        
        public CommandRef ExportCsv { get; set; }
        public CommandRef EditProfiles { get; set; }
        public CommandRef SaveFileDialogCsv { get; set; }
        
        public MainViewModel()
        {
            
            DataFiles.CollectionChanged += DataFiles_CollectionChanged;
            Profiles.CollectionChanged += Profiles_CollectionChanged;
            PropertyChanged += SelectedAny_PropertyChanged;
            
            // Загружаем профили, если они есть
            if (File.Exists(ProfileFileName))
            {
                _profileSaver.Open(ref _profiles);
                NotifyPropertyChanged("Profiles");
            }
            else
            {
                Profiles = new ObservableCollection<Profile>();
                Profiles.Add( new Profile() { Name = "NewProfile" } );
                _profileSaver.Save(_profiles);
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
            
            ExportCsv = new CommandRef((args) => { ExportCsvMethod(); });
            ExportCsv.CanExecuteDelegate = (arg)=>{
                return  (SelectedElementsTable != null) &&
                        (SelectedProfile != null) &&
                        (SelectedStructureHeader != "");
            };

            EditProfiles = new CommandRef((args) => { EditProfilesMethod(); });
            SaveFileDialogCsv = new CommandRef((args) => { SaveFileDialogCsvMethod(); });
            
            _mw.DataContext = this;
            _mw.Show();
            _initialized = true;
        }
        
        /// <summary>
        /// Проверка правильности конфигураций
        /// </summary>
        /// <returns>Есть ошибки при проверке</returns>
        private bool CheckConfiguration()
        {
            var lastTableName = "";
            var err = false;

            if (SelectedElementsTable.Table.Rows.Count == 0){
                Log("Таблица экземпляров " + SelectedElementsTable.Name + " не содержит экземпляров");
                err = true;
            }
            
            foreach(DataRow edr in SelectedElementsTable.Table.Rows){
                var tmpTableName = edr[SelectedStructureHeader].ToString();
                // ссылка на структуру пуста
                if (tmpTableName == "") {
                    Log("Не указано имя структуры для элемента: " + edr[0]);
                    err = true;
                    continue; }

                // все остальные тесты касаются структуры, если она посвторяется пропускаем
                if (tmpTableName == lastTableName) continue;
                    
                lastTableName = tmpTableName;
                // все используемые структуры существуют
                if (GetTableByName(tmpTableName) == null){
                    Log("Указанная структура для элемента: " + edr[0] + " отсутствует строка: " + SelectedElementsTable.Table.Rows.IndexOf(edr).ToString());
                    err = true;
                    continue;}

                // все используемые структуры имеют все требуемые профилем заголовки
                foreach(string cName in (from c in SelectedProfile.Rules where c.MastExist select c.Name ))
                    if (!GetTableByName(tmpTableName).Headers.Contains(cName)){
                        Log("Cтруктура: " + tmpTableName + " не содержит обязательного заголовка " + cName);
                        err |= true;
                    }

            }// end_foreach
                

            // ???? все используемые структуры имеют одинаковый заголовок (последовательность не важна)
            return err;    
        }

        /// <summary>
        /// процедура экспорта
        /// </summary>
        private void ExportCsvMethod()
        {
            InnerTable strTmp;

            Errors.Clear();
            if (CheckConfiguration())
            {
                Log("Сборка отменена, из-за ошибок.");
                return;
            }

            if (SelectedExportFile == "")
                if (!SaveFileDialogCsvMethod())
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
                var tmp="";
                foreach (DataColumn c in strTmp.Table.Columns) 
                    tmp += c.ColumnName + SelectedProfile.FieldSeparator;                
                File.WriteAllText(SelectedExportFile, tmp.Substring(0, tmp.Length - 1) + "\r\n",SelectedProfile.Encoding);
            }

            // создаем экспортер
            ExporterCSV expCsv = new ExporterCSV(SelectedExportFile, SelectedProfile);
            expCsv.RiseLogMessageEvent += Log;

            // перебираем элементы и записываем
            try
            {
                foreach (DataRow eDr in SelectedElementsTable.Table.Rows)
                {
                    strTmp = GetTableByName(eDr[SelectedStructureHeader].ToString());
                    var resTmpTable = expCsv.SubstituteStructureCopy(eDr, strTmp.Table);

                    //Проверяем на правила и записываем в файл
                    File.AppendAllLines(SelectedExportFile, expCsv.MakeStrings(resTmpTable), SelectedProfile.Encoding);                    
                }
                Log("Сборка завершена успешно");
            }
            catch(Exception ex)
            { Log("Сборка прервана из-за ошибки: " + ex.Message); }
            
        }//void ExportCSVMethod()

        /// <summary>
        /// Поиск таблицы по имени
        /// </summary>
        /// <param name="name">имя таблицы</param>
        /// <returns></returns>
        private InnerTable GetTableByName(string name)
        {
            foreach (var t in SelectedElementsTable.Parent.Tables)
                if (t.SearchName == name) return t;

            foreach (var t in _allTables)
                if (t.SearchName == name) return t;

            return null;
        }

        /// <summary>
        /// Записать в лог
        /// </summary>
        /// <param name="message">сообщение</param>
        void Log(string message){
            Errors.Add(message);
            if(_mw.BottomRow.Height.Value<20)
                _mw.BottomRow.Height = new GridLength(100);
        }

        private bool SaveFileDialogCsvMethod()
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
            ProfileEditorViewModel pevm = new ProfileEditorViewModel(this);
            pevm.OnDone += () =>
            {
                _profileSaver.Save(_profiles);
            };

        }
        
        // Изменение коллекции файлов
        private void DataFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var ni in e.NewItems) _allTables.AddRange(((DataFile)ni).Tables);
            if (e.OldItems != null)
                foreach (var oi in e.OldItems) _allTables.RemoveRange(((DataFile)oi).Tables);

            NotifyPropertyChanged("AllTables");
            NotifyPropertyChanged("TreeTipVisibility");

        }

        private void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _profileSaver.Save(_profiles);
        }

        private void SelectedAny_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_initialized) return;

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
                _profileSaver.Save(_profiles);

            Properties.Settings.Default.Save();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Реализация интерфейса INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }//MainViewModel
}
