using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace CSVMaker.Model
{
    /// <summary>
    /// Класс позволяющий сохранять файлы
    /// </summary>
    public class Saver
    {
        public string Ext = "xml";

        public string FileName { get; set; }

        /// <summary>
        /// Вызывает диалог открытия файлов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Объект куда десериализуется файл</param>
        /// <returns>1 - успешно, 0 - ошибка</returns>
        public bool OpenAs<T>(ref T obj)
        //    where T : class, IEnumerable, ICollection
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*."+ Ext+"|*." + Ext;
            if(ofd.ShowDialog()==true)
            {
                FileName = ofd.FileName;
                return Open(ofd.FileName,ref obj);
            }
            return false;
        }

        /// <summary>
        /// Вызывает диалог сохранения файла
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Объект откуда сериализуется файл</param>
        /// <returns>1 - успешно, 0 - ошибка</returns>
        public bool SaveAs<T>( T obj)
        //    where T : class, IEnumerable, ICollection
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*." + Ext + "|*." + Ext;
            if (sfd.ShowDialog() == true)
            {
                FileName = sfd.FileName;
                return Save(sfd.FileName, obj);
            }
            return false;
        }

        /// <summary>
        /// Открывает указанный файл и десериализует его в obj
        /// </summary>
        /// <typeparam name="T">Тип десиреализуемого объекта</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="obj">Объект в который десериализуется файл</param>
        /// <returns>true - Объект был упешно восстановлен из файла. 0 - Возникли проблемы.</returns>
        public bool Open<T>(ref T obj)
        //    where T:class, IEnumerable, ICollection
        {
            if (File.Exists(FileName))
            {
                try
                {
                    TextReader textWriter = new StreamReader(FileName);
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    obj = (T)serializer.Deserialize(textWriter);
                    textWriter.Close();
                    return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// Открывает указанный файл и десериализует его в obj
        /// </summary>
        /// <typeparam name="T">Тип десиреализуемого объекта</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="obj">Объект в который десериализуется файл</param>
        /// <returns>true - Объект был упешно восстановлен из файла. 0 - Возникли проблемы.</returns>
        public bool Open<T>(string fileName, ref  T obj)
        //    where T : class, IEnumerable, ICollection
        {
            FileName = fileName;
            return Open(ref obj);
        }
        
        /// <summary>
        /// Сохраняет переданный объект в указанный файл
        /// </summary>
        /// <typeparam name="T">Тип сиреализуемого объекта</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="obj">Объект который сериализуется в файл</param>
        /// <returns>true - Объект был упешно восстановлен из файла. 0 - Возникли проблемы.</returns>
        public bool Save<T>( T obj)
        //    where T : class, IEnumerable, ICollection
        {
            //if (File.Exists(FileName))
            //    if (System.Windows.Forms.MessageBox.Show("Перезаписать " + FileName + "?", "", Windows.Forms.MessageBoxButtons.YesNo) != Windows.Forms.DialogResult.Yes)
            //        return false;
            try
            {
                StreamWriter textWriter = File.CreateText(FileName);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(textWriter, obj);
                textWriter.Close();
                return true;
            }
            catch 
            {
            }
            return false;
        }

        /// <summary>
        /// Сохраняет переданный объект в указанный файл
        /// </summary>
        /// <typeparam name="T">Тип сиреализуемого объекта</typeparam>
        /// <param name="fileName">Путь к файлу</param>
        /// <param name="obj">Объект который сериализуется в файл</param>
        /// <returns>true - Объект был упешно восстановлен из файла. 0 - Возникли проблемы.</returns>
        public bool Save<T>(string fileName, T obj)
        //    where T : class , IEnumerable, ICollection
        {
            FileName = fileName;
            return Save( obj);
        }
    }//class Saver
}
