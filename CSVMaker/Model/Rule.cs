using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSVMaker.Model
{
    /// <summary>
    /// правила проверки
    /// </summary>
    public class Rule : INotifyPropertyChanged, ICloneable
    {
        #region Fields & Props
        [DisplayName("Ватрушка")]
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string   RegExp { get; set; }
        [XmlAttribute]
        public int      MaxLength { get; set; }
        [XmlAttribute]
        public HandleQuotesType HandleQuotesType { get; set; }
        [XmlAttribute]
        public bool     MastExist { get; set; }
        [XmlAttribute]
        public bool     Ignore { get; set; }
        #endregion

        #region Constructor
        public Rule()
        {
            Name = "Поле";
            RegExp = @"^[A-zА-яЁё0-9 \.]*$";
            MaxLength = 512;
            HandleQuotesType = HandleQuotesType.None;
        }
        #endregion

        public void HandleQuotes(ref string FieldVal){
            switch (HandleQuotesType)
            {
                case HandleQuotesType.Add:
                    while (FieldVal.StartsWith("\"") || FieldVal.EndsWith("\""))
                    {
                        FieldVal = FieldVal.StartsWith("\"") ? FieldVal.Substring(1, FieldVal.Length - 1) : FieldVal;
                        FieldVal = FieldVal.EndsWith("\"") ? FieldVal.Substring(0, FieldVal.Length - 1) : FieldVal;
                    }
                    FieldVal = "\"" + FieldVal + "\"";
                    break;
                case HandleQuotesType.AddRemoveAll:
                    FieldVal = "\"" + FieldVal.Replace("\"","") + "\"";
                    break;
                case HandleQuotesType.Remove:
                    while (FieldVal.StartsWith("\"") || FieldVal.EndsWith("\""))
                    {
                        FieldVal = FieldVal.StartsWith("\"") ? FieldVal.Substring(1, FieldVal.Length - 1) : FieldVal;
                        FieldVal = FieldVal.EndsWith("\"") ? FieldVal.Substring(0, FieldVal.Length - 1) : FieldVal;
                    }
                    break;
                case HandleQuotesType.RemoveAll:
                    FieldVal = FieldVal.Replace("\"", "");
                    break;
                default:
                    break;
            }
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
        #endregion

        /// <summary>
        /// Для реализации клонирования
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Rule() {
                Name = this.Name,
                RegExp = this.RegExp,
                MaxLength = this.MaxLength,
                HandleQuotesType = this.HandleQuotesType,
                Ignore = this.Ignore
            };
        }
    }// Rule


    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HandleQuotesType
    {
        [Description("Не обрабатывать")]
        None,
        [Description("Добавить ^$")]
        Add,
        [Description("Добавить ^$, удалить везде")]
        AddRemoveAll,
        [Description("Удалить ^$")]
        Remove,
        [Description("Удалить везде")]
        RemoveAll
    }
}
