using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CSVMaker.ViewModel
{
    public class TesterViewModel : INotifyPropertyChanged
    {
        private static readonly Microsoft.JScript.Vsa.VsaEngine JsEngine = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();

        private string _regExp = @"^[A-zА-яЁё0-9 \.]*$";
        public string RegExp
        {
            get => _regExp;
            set { _regExp = value; NotifyPropertyChanged(); NotifyPropertyChanged("RegExpResult"); }
        }

        private string _testTxt = "Проверяемый текст ячейки с цифрами 0,2";
        public string TestTxt
        {
            get => _testTxt;
            set { _testTxt = value; NotifyPropertyChanged(); NotifyPropertyChanged("RegExpResult"); }
        }

        private string _code = "'= \"%MW\" + 1 + 2 + (1 + 2)";
        public string Code
        {
            get => _code;
            set { _code = value; NotifyPropertyChanged(); NotifyPropertyChanged("CodeResult"); }
        }

        public string RegExpResult
        {
            get {
                try { return Regex.IsMatch(_testTxt, RegExp) ? "Совпадает" : "Не совпадает"; }
                catch { return "Ошибка в выражении"; }
            }
        }

        public string CodeResult
        {
            get
            {
                if (!(_code.StartsWith("'=") || _code.StartsWith("="))) return "Формула должна начинаться с \"'=\" или с \"=\"";
                try { return Microsoft.JScript.Eval.JScriptEvaluate(_code.Replace("'=", "").Replace("=", ""), JsEngine).ToString(); }
                catch { return "Ошибка в формуле"; }
            }
        }

        public List<RegExpExample> RegExpExamples { get; set; } = new List<RegExpExample>();
        
        public TesterViewModel()
        {
            RegExpExamples.Add(new RegExpExample() {RegExp = @"^[A-zА-яЁё0-9 \.]*$", Description = "Содержит все буквы кириллицы и латиницы и цыфры пробел и точку. Может быть пустым." });
            RegExpExamples.Add(new RegExpExample() { RegExp = @"^[A-zА-яЁё0-9 \.]+$", Description = "Содержит все буквы кириллицы и латиницы и цыфры пробел и точку. Не может быть пустым." });
            RegExpExamples.Add(new RegExpExample() { RegExp = @"^[0-9\-\.]+$", Description = "Содержит цыфры, точку знак минус. Не может быть пустым." });

            var tw = new View.Tester {DataContext = this};
            tw.Show();
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
    }

    public class RegExpExample
    {
        public string RegExp { get; set; }
        public string Description { get; set; }
    }
}
