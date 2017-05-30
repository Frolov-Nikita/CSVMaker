using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSVMaker.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// драгдроп файла в дерево-источник таблиц
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_Drop(object sender, DragEventArgs e)
        {
            foreach (var fn in (string[])e.Data.GetData(DataFormats.FileDrop))
                ((ViewModel.MainViewModel)DataContext).DataFiles.Add(new Model.DataFile(fn));
        }

        /// <summary>
        /// изменение по клику высоты столбца с логами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(BottomRow.Height.Value ==0)
                BottomRow.Height = new GridLength(100);
            else
                BottomRow.Height = new GridLength(0);
        }

        /// <summary>
        /// Изменение по двойному клику ширины дерево-источник таблиц 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridSplitter_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (LeftTreeColumn.Width.Value == 0)
                LeftTreeColumn.Width = new GridLength(180);
            else
                LeftTreeColumn.Width = new GridLength(0);

        }

        /// <summary>
        /// При двойном клике открываем диалог открытия
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = "Excel Files|*.xls*|CSV Files|*.csv",
                Title = "Выберите источники таблиц",
                CheckFileExists = true,
            };
            if(ofd.ShowDialog() == true)
            {
                foreach (var fn in ofd.FileNames)
                    ((ViewModel.MainViewModel)DataContext).DataFiles.Add(new Model.DataFile(fn));
            }
        }

        /// <summary>
        /// Удаление выделенного файла кнопкой Delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete & ((TreeView)sender).SelectedItem.GetType()==typeof(Model.DataFile))
            {
                ((ViewModel.MainViewModel)DataContext).DataFiles.Remove((Model.DataFile)((TreeView)sender).SelectedItem);
            }
        }
        
    }
}
