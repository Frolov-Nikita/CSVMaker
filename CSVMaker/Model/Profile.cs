using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace CSVMaker.Model
{
    public class Profile : INotifyPropertyChanged, ICloneable
    {
        #region Fields & Props
        string _Name = "";
        /// <summary>
        /// Название профиля
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; NotifyPropertyChanged("Name"); }
        }

        [XmlAttribute]
        public HeaderType HeaderType { get; set; }
        
        string _CustomHeader;
        /// <summary>
        /// Заголовок, который свтавляется перед табличной частью
        /// </summary>
        public string CustomHeader
        {
            get { return _CustomHeader; }
            set { _CustomHeader = value; NotifyPropertyChanged("CustomHeader"); }
        }
        
        string _FieldSeparator = ";";
        /// <summary>
        /// Символ разделения столбцов таблицы
        /// </summary>
        [XmlAttribute]
        public string FieldSeparator
        {
            get { return _FieldSeparator; }
            set { _FieldSeparator = value; NotifyPropertyChanged("FieldSeparator"); }
        }
        
        ObservableCollection<Rule> _Rules = new ObservableCollection<Rule>();
        /// <summary>
        /// Правила для столбцев
        /// </summary>
        public ObservableCollection<Rule> Rules
        {
            get { return _Rules; }
            set { _Rules = value; NotifyPropertyChanged("Rules"); }
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
            get {return Encoding.WebName; }
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
        static Microsoft.JScript.Vsa.VsaEngine jsEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();
        
        #endregion

        #region Constructor
        public Profile()
        {            
            this.Name = "Default";
            this.CustomHeader = "";
            this.FieldSeparator = ";";
            this.Encoding = Encoding.UTF8;
            this.Rules = new ObservableCollection<Rule>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Проверяет, вычисляет и обрабатывает значение
        /// </summary>
        /// <param name="FieldName">Названия правила/поля</param>
        /// <param name="FieldValue">Проверяемое значение</param>
        /// <returns>Сообщение об ошибке</returns>
        public string Assert(string FieldName, ref string FieldValue)
        {
            // Вычисляем формулу, если она есть
            if (FieldValue.StartsWith("'=") || FieldValue.StartsWith("="))
            {// тут формула 
                try { FieldValue = Microsoft.JScript.Eval.JScriptEvaluate(FieldValue.Replace("'=", "").Replace("=", ""), jsEngine).ToString(); }
                catch { return "Неправильная формула в поле " + FieldName; }
            }

            foreach (Rule r in Rules)
                if ((r.Name == FieldName) && (!r.Ignore))
                {
                    // Проверка на длину
                    if (FieldValue.Length > r.MaxLength)  return "Длянна поля " + FieldName + " Больше " + r.MaxLength.ToString(); 

                    // Проверка на regexp
                    try
                    {
                        if(r.RegExp != "")
                            if (!Regex.IsMatch(FieldValue, r.RegExp)) return "Поля " + FieldName + " содержит недопустимые символы";
                    }
                    catch { return "Некорректное правило проверки на допустимые символы!"; }

                    // обработка кавычек
                    r.HandleQuotes(ref FieldValue);
                }//end_if
            return "";
        }//AssertField
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

        public object Clone()
        {
            Profile r =  new Profile()
            {
                Name = this.Name,
                Encoding = this.Encoding,
                HeaderType = this.HeaderType,
                CustomHeader = this.CustomHeader,
                FieldSeparator = this.FieldSeparator                
            };
            foreach (Rule rul in Rules) r.Rules.Add((Rule)rul.Clone());
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
