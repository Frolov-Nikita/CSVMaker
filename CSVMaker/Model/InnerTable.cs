using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Runtime.CompilerServices;

namespace CSVMaker.Model
{
    //GetTables

    /// <summary>
    /// Таблица в файле с данными
    /// </summary>
    public class InnerTable : INotifyPropertyChanged,IDisposable
    {
        public string Name { get; set; }
        public string SearchName { get; set; }
        public DataFile Parent { get; set; }
        public ObservableCollection<string> Headers { get; set; } = new ObservableCollection<string>();

        DataTable _table;
        public DataTable Table{ get{
                if (_table == null)
                {
                    _table = new DataTable();
                    var da = new OleDbDataAdapter($"SELECT * FROM [{Name}]", Parent.Connection);
                    da.Fill(_table);
                    da.Dispose();
                }
                return _table;
            }
        }

        public InnerTable(DataFile parent, string tableName) {
            Parent = parent;
            Name = tableName;
            SearchName = tableName.Replace("$", "").Replace("#", "").Replace("^", "").Replace("'", "");
            GetHeaders();
        }

        void GetHeaders(){
            string q = $"SELECT TOP 1 * FROM [{Name}]";
            var da = new OleDbDataAdapter(q, Parent.Connection);
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
            comm.CommandText = $"SELECT * FROM [{Name}]";
            return comm.ExecuteReader();
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
            _table.Dispose();
            //throw new NotImplementedException();
        }
    }//DataTable
}
