using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSVMaker.Model;
using System.Text.RegularExpressions;

namespace CSVMaker.ViewModel
{
    public class TesterViewModel : INotifyPropertyChanged
    {
        #region Fields & Propertyes
        static Microsoft.JScript.Vsa.VsaEngine jsEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();

        string _RegExp = @"^[A-zА-яЁё0-9 \.]*$";
        public string RegExp
        {
            get { return _RegExp; }
            set { _RegExp = value; NotifyPropertyChanged("RegExp"); NotifyPropertyChanged("RegExpResult"); }
        }

        string _TestTxt = "Проверяемый текст ячейки с цифрами 0,2";
        public string TestTxt
        {
            get { return _TestTxt; }
            set { _TestTxt = value; NotifyPropertyChanged("TestTxt"); NotifyPropertyChanged("RegExpResult"); }
        }

        string _Code = "'= \"%MW\" + 1 + 2 + (1 + 2)";
        public string Code
        {
            get { return _Code; }
            set { _Code = value; NotifyPropertyChanged("Code"); NotifyPropertyChanged("CodeResult"); }
        }

        public string RegExpResult
        {
            get {
                try { return Regex.IsMatch(_TestTxt, RegExp) ? "Совпадает" : "Не совпадает"; }
                catch { return "Ошибка в выражении"; }
            }
        }

        public string CodeResult
        {
            get
            {
                if (!(_Code.StartsWith("'=") || _Code.StartsWith("="))) return "Формула должна начинаться с \"'=\" или с \"=\"";
                try { return Microsoft.JScript.Eval.JScriptEvaluate(_Code.Replace("'=", "").Replace("=", ""), jsEngine).ToString(); }
                catch { return "Ошибка в формуле"; }
            }
        }

        public List<RegExpExample> RegExpExamples { get; set; } = new List<RegExpExample>();
        #endregion

        #region Конструктор
        public TesterViewModel()
        {
            #region список примеров
            RegExpExamples.Add(new RegExpExample() {RegExp = @"^[A-zА-яЁё0-9 \.]*$", Description = "Содержит все буквы кириллицы и латиницы и цыфры пробел и точку. Может быть пустым." });
            RegExpExamples.Add(new RegExpExample() { RegExp = @"^[A-zА-яЁё0-9 \.]+$", Description = "Содержит все буквы кириллицы и латиницы и цыфры пробел и точку. Не может быть пустым." });
            RegExpExamples.Add(new RegExpExample() { RegExp = @"^[0-9\-\.]+$", Description = "Содержит цыфры, точку знак минус. Не может быть пустым." });
            #endregion

            CSVMaker.View.Tester tw = new View.Tester();
            tw.DataContext = this;
            tw.Show();
        }

        
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
    }

    public class RegExpExample
    {
        public string RegExp { get; set; }
        public string Description { get; set; }
    }
}
