using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CSVMaker.Model
{
    /// <summary>
    /// Файл я данными
    /// </summary>
    public class DataFile: INotifyPropertyChanged, IDisposable
    {
        string _Name = "";
        public string Name {
            get { return _Name; }
            set {
                _Name = value;
                NotifyPropertyChanged("Name");
                GetTables();
                ; } }

        ObservableCollection<InnerTable> _Tables = new ObservableCollection<InnerTable>();
        public ObservableCollection<InnerTable> Tables { get { return _Tables; } }

        OleDbConnection _Connection = null;
        public OleDbConnection Connection { get {
                if (_Connection != null)
                {
                    if (_Connection.State == ConnectionState.Broken)
                    { _Connection.Close(); _Connection.Dispose(); _Connection = null; }

                    if ((_Connection.State == ConnectionState.Executing) ||
                        (_Connection.State == ConnectionState.Connecting) ||
                        (_Connection.State == ConnectionState.Fetching))
                    { Thread.Sleep(500); }

                    if ((_Connection.State == ConnectionState.Executing) ||
                        (_Connection.State == ConnectionState.Connecting) ||
                        (_Connection.State == ConnectionState.Fetching))
                    { _Connection.Close(); _Connection.Dispose(); _Connection = null; }
                }
                if (_Connection == null)
                {
                    var connectionString = "";
                    var fileNameLower = Name.ToLower();

                    if (fileNameLower.EndsWith("xlsx") ||
                        fileNameLower.EndsWith("xlsm"))
                    {
                        connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0", Name);
                    }
                    else if (fileNameLower.EndsWith("xlsb"))
                    {
                        connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0", Name);
                    }
                    else if (fileNameLower.EndsWith("csv"))
                    {
                        connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""text;Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0", Path.GetDirectoryName(Name));
                    }
                    else
                    {
                        connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0", Name);
                    }
                    _Connection = new OleDbConnection(connectionString);
                    
                }

                if (_Connection.State != ConnectionState.Open) _Connection.Open();
                return _Connection; } }

        public DataFile(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Строка подключения по имени файла
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        OleDbConnection GetConnection()
        {
            var connectionString = "";
            var fileNameLower = Name.ToLower();

            if (fileNameLower.EndsWith("xlsx") ||
                fileNameLower.EndsWith("xlsm"))
            {
                connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0 Xml;HDR=YES;IMEX=1""", Name);
            }
            else if (fileNameLower.EndsWith("xlsb"))
            {
                connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;HDR=YES;IMEX=1""", Name);
            }
            else if (fileNameLower.EndsWith("csv"))
            {
                connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""text;Excel 12.0;HDR=YES;IMEX=1""", Path.GetDirectoryName(Name));
            }
            else
            {
                connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=""Excel 12.0;HDR=YES;IMEX=1""", Name);
            }
            return new OleDbConnection(connectionString);
        }//GetConnectionString
        
        void GetTables()
        {
            _Tables.Clear();

            //if (Connection.State == ConnectionState.Closed) Connection.Open();

            var SchemaTables = Connection.GetOleDbSchemaTable( OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });

            _Tables.AddRange(from DataRow row in SchemaTables.Rows select new InnerTable(this, row["TABLE_NAME"].ToString()));
            Connection.Close();
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
            _Connection.Dispose();
        }

        #endregion
    }//GetTables

    /// <summary>
    /// Таблица в файле с данными
    /// </summary>
    public class InnerTable : INotifyPropertyChanged,IDisposable
    {
        public string Name { get; set; }
        public string SearchName { get; set; }
        public DataFile Parent { get; set; }
        public ObservableCollection<string> Headers { get; set; } = new ObservableCollection<string>();

        DataTable _Table = null;
        public DataTable Table{ get{
                if (_Table == null)
                {
                    _Table = new DataTable();
                    var da = new OleDbDataAdapter(String.Format("SELECT * FROM [{0}]",Name), Parent.Connection);
                    da.Fill(_Table);
                    da.Dispose();
                }
                return _Table;
            }
        }

        public InnerTable(DataFile parent, string TableName) {
            Parent = parent;
            Name = TableName;
            SearchName = TableName.Replace("$", "").Replace("#", "").Replace("^", "").Replace("'", "");
            GetHeaders();
        }

        void GetHeaders(){
            string Q = string.Format("SELECT TOP 1 * FROM [{0}]", Name);
            var da = new OleDbDataAdapter(Q, Parent.Connection);
            DataTable dataTable = new DataTable();
            da.Fill(dataTable);
            Headers.Clear();
            foreach (var item in dataTable.Columns)
            {
                Headers.Add(item.ToString());
            }
        }

        public OleDbDataReader GetItemsReader(){
            var comm = Parent.Connection.CreateCommand();
            comm.CommandText = string.Format("SELECT * FROM [{0}]",Name);
            return comm.ExecuteReader();
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void Dispose()
        {
            _Table.Dispose();
            //throw new NotImplementedException();
        }
        #endregion
    }//DataTable
}
