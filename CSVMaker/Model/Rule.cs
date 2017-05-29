using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        public Rule()
        {
            Name = "Поле";
            RegExp = @"^[A-zА-яЁё0-9 \.]*$";
            MaxLength = 512;
            HandleQuotesType = HandleQuotesType.None;
        }

        public void HandleQuotes(ref string fieldVal){
            switch (HandleQuotesType)
            {
                case HandleQuotesType.Add:
                    while (fieldVal.StartsWith("\"") || fieldVal.EndsWith("\""))
                    {
                        fieldVal = fieldVal.StartsWith("\"") ? fieldVal.Substring(1, fieldVal.Length - 1) : fieldVal;
                        fieldVal = fieldVal.EndsWith("\"") ? fieldVal.Substring(0, fieldVal.Length - 1) : fieldVal;
                    }
                    fieldVal = "\"" + fieldVal + "\"";
                    break;
                case HandleQuotesType.AddRemoveAll:
                    fieldVal = "\"" + fieldVal.Replace("\"","") + "\"";
                    break;
                case HandleQuotesType.Remove:
                    while (fieldVal.StartsWith("\"") || fieldVal.EndsWith("\""))
                    {
                        fieldVal = fieldVal.StartsWith("\"") ? fieldVal.Substring(1, fieldVal.Length - 1) : fieldVal;
                        fieldVal = fieldVal.EndsWith("\"") ? fieldVal.Substring(0, fieldVal.Length - 1) : fieldVal;
                    }
                    break;
                case HandleQuotesType.RemoveAll:
                    fieldVal = fieldVal.Replace("\"", "");
                    break;
                default:
                    break;
            }
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
