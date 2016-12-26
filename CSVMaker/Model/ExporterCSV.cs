using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVMaker.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ExporterCSV
    {
        string FileName;
        Profile Profile;
        List<DataColumn> ColumnOrder = null;
        
        public ExporterCSV(string fileName, Profile profile)
        {
            FileName = fileName;
            Profile = profile;
        }
        
        /// <summary>
        /// Делает подстановки и вычисляет формулы
        /// </summary>
        /// <param name="e">Елемент</param>
        /// <param name="str">Структура элемента</param>
        /// <returns></returns>
        public DataTable SubstituteStructureCopy(DataRow e, DataTable str)
        {
            DataTable rDT = str.Copy();

            foreach (DataRow strRow in rDT.Rows) // строки структуры
                foreach (DataColumn strCol in rDT.Columns) // столбцы структуры
                    foreach (DataColumn ec in e.Table.Columns) // столбцы экземпляра
                        strRow[strCol] = strRow[strCol].ToString().Replace("{" + ec.ColumnName + "}", e[ec].ToString());
                    
            return rDT;
        }// DataTable SubstituteStructure(..)

        /// <summary>
        /// Создает массив строк по таблицке
        /// </summary>
        /// <param name="dt">тыбличка</param>
        /// <returns>Массив строк</returns>
        public List<string> MakeStrings(DataTable dt)
        {
            List<string> rLS = new List<string>();
            string tmpLine = "", tmpErr, tmpCell;

            //создадим порядок колонок
            if (ColumnOrder == null){
                ColumnOrder = new List<DataColumn>();
                foreach (DataColumn c in dt.Columns) ColumnOrder.Add(c);
            }                

            foreach (DataRow dtRow in dt.Rows){
                tmpLine = "";
                foreach (DataColumn c in ColumnOrder){
                    tmpCell = dtRow.Table.Columns.Contains(c.ColumnName)? dtRow[c.ColumnName].ToString():"";
                    tmpErr = Profile.Assert(c.ColumnName, ref tmpCell);
                    tmpLine += tmpCell + (ColumnOrder.Last() != c ? Profile.FieldSeparator : "");
                    if (tmpErr != "") RiseLogMessageEvent(tmpErr);
                }
                rLS.Add(tmpLine);
            }
            return rLS;
        }

        public delegate void RiseLogMessage(string Message);
        public event RiseLogMessage RiseLogMessageEvent;
        
    }// class ExporterCSV
    
}
