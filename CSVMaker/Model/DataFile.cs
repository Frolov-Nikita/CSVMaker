using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSVMaker.Model
{
    /// <summary>
    /// Файл я данными
    /// </summary>
    public class DataFile: INotifyPropertyChanged, IDisposable
    {
        string _name = "";
        public string Name {
            get => _name;
            set {
                _name = value;
                NotifyPropertyChanged();
                GetTables();
            } }

        ObservableCollection<InnerTable> _tables = new ObservableCollection<InnerTable>();
        public ObservableCollection<InnerTable> Tables => _tables;

        OleDbConnection _connection;
        public OleDbConnection Connection { get {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Broken)
                { _connection.Close(); _connection.Dispose(); _connection = null; }

                if (_connection != null && ((_connection.State == ConnectionState.Executing) ||
                                            (_connection.State == ConnectionState.Connecting) ||
                                            (_connection.State == ConnectionState.Fetching)))
                { Thread.Sleep(500); }

                if (_connection != null && ((_connection.State == ConnectionState.Executing) ||
                                            (_connection.State == ConnectionState.Connecting) ||
                                            (_connection.State == ConnectionState.Fetching)))
                { _connection.Close(); _connection.Dispose(); _connection = null; }
            }
            if (_connection == null)
            {
                string connectionString;
                var fileNameLower = Name.ToLower();

                if (fileNameLower.EndsWith("xlsx") ||
                    fileNameLower.EndsWith("xlsm"))
                {
                    connectionString =
                        $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={
                                Name
                            };Extended Properties=""Excel 12.0 Xml;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0";
                }
                else if (fileNameLower.EndsWith("xlsb"))
                {
                    connectionString =
                        $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={
                                Name
                            };Extended Properties=""Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0";
                }
                else if (fileNameLower.EndsWith("csv"))
                {
                    connectionString =
                        $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={
                                Path.GetDirectoryName(Name)
                            };Extended Properties=""text;Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0";
                }
                else
                {
                    connectionString =
                        $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={
                                Name
                            };Extended Properties=""Excel 12.0;HDR=YES;IMEX=1"";Jet OLEDB:Database Locking Mode=0";
                }
                _connection = new OleDbConnection(connectionString);
                    
            }

            if (_connection.State != ConnectionState.Open) _connection.Open();
            return _connection; } }

        public DataFile(string name)
        {
            Name = name;
        }
        
        private void GetTables()
        {
            _tables.Clear();
            
            var schemaTables = Connection.GetOleDbSchemaTable( OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });

            _tables.AddRange(from DataRow row in schemaTables.Rows select new InnerTable(this, row["TABLE_NAME"].ToString()));
            Connection.Close();
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

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}