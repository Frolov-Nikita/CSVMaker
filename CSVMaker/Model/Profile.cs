using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace CSVMaker.Model
{
    public class Profile : INotifyPropertyChanged, ICloneable
    {
        string _name = "";
        /// <summary>
        /// Название профиля
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get => _name;
            set { _name = value; NotifyPropertyChanged(); }
        }

        [XmlAttribute]
        public HeaderType HeaderType { get; set; }
        
        string _customHeader;
        /// <summary>
        /// Заголовок, который свтавляется перед табличной частью
        /// </summary>
        public string CustomHeader
        {
            get => _customHeader;
            set { _customHeader = value; NotifyPropertyChanged(); }
        }
        
        string _fieldSeparator = ";";
        /// <summary>
        /// Символ разделения столбцов таблицы
        /// </summary>
        [XmlAttribute]
        public string FieldSeparator
        {
            get => _fieldSeparator;
            set { _fieldSeparator = value; NotifyPropertyChanged(); }
        }
        
        ObservableCollection<Rule> _rules = new ObservableCollection<Rule>();
        /// <summary>
        /// Правила для столбцев
        /// </summary>
        public ObservableCollection<Rule> Rules
        {
            get => _rules;
            set { _rules = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Кодировка конечного файла
        /// </summary>
        [XmlIgnore]
        public Encoding Encoding {get;set;}

        /// <summary>
        /// Кодировка для сериализации
        /// </summary>
        [XmlAttribute]
        public string EncodingName {
            get => Encoding.WebName;
            set { Encoding = Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Список кодировок
        /// </summary>
        [XmlIgnore]
        public static List<Encoding> Encodings { get; set; } = new List<Encoding>() {
            Encoding.GetEncoding("utf-7"),
            Encoding.GetEncoding("utf-8"),
            Encoding.GetEncoding("utf-16"),
            Encoding.GetEncoding("utf-32"),
            Encoding.GetEncoding("utf-32BE"),
            Encoding.GetEncoding("windows-1250"),
            Encoding.GetEncoding("windows-1251"),
            Encoding.GetEncoding("Windows-1252"),
            Encoding.GetEncoding("cp866"),
            Encoding.GetEncoding("iso-8859-5"),
            Encoding.GetEncoding("koi8-u"),
            Encoding.GetEncoding("koi8-r")
        };

        [XmlIgnore]
        static readonly Microsoft.JScript.Vsa.VsaEngine JsEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();
        
        public Profile()
        {
            Name = "Default";
            CustomHeader = "";
            FieldSeparator = ";";
            Encoding = Encoding.UTF8;
            Rules = new ObservableCollection<Rule>();
        }
        
        /// <summary>
        /// Проверяет, вычисляет и обрабатывает значение
        /// </summary>
        /// <param name="fieldName">Названия правила/поля</param>
        /// <param name="fieldValue">Проверяемое значение</param>
        /// <returns>Сообщение об ошибке</returns>
        public string Assert(string fieldName, ref string fieldValue)
        {
            // Вычисляем формулу, если она есть
            if (fieldValue.StartsWith("'=") || fieldValue.StartsWith("="))
            {// тут формула 
                try { fieldValue = Microsoft.JScript.Eval.JScriptEvaluate(fieldValue.Replace("'=", "").Replace("=", ""), JsEngine).ToString(); }
                catch { return "Неправильная формула в поле " + fieldName; }
            }

            foreach (var r in Rules)
                if ((r.Name == fieldName) && (!r.Ignore))
                {
                    // Проверка на длину
                    if (fieldValue.Length > r.MaxLength)  return "Длянна поля " + fieldName + " Больше " + r.MaxLength.ToString(); 

                    // Проверка на regexp
                    try
                    {
                        if(r.RegExp != "")
                            if (!Regex.IsMatch(fieldValue, r.RegExp)) return "Поля " + fieldName + " содержит недопустимые символы";
                    }
                    catch { return "Некорректное правило проверки на допустимые символы!"; }

                    // обработка кавычек
                    r.HandleQuotes(ref fieldValue);
                }//end_if
            return "";
        }//AssertField
        
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Реализация интерфейса INotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            Profile r =  new Profile()
            {
                Name = Name,
                Encoding = Encoding,
                HeaderType = HeaderType,
                CustomHeader = CustomHeader,
                FieldSeparator = FieldSeparator                
            };
            foreach (var rul in Rules) r.Rules.Add((Rule)rul.Clone());
            return r;
        }

    }// Class Profile

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HeaderType {
        [Description("Без заголовка")]
        None,
        [Description("Взять из первой структуры")]
        FromStruct,
        [Description("Задать в ручную")]
        Custom }
    
}
