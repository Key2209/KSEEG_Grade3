
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static KSEEG_Fversion_Lib.BleManager;
using EEG.Tool;
using EEG.ViewModel;
namespace EEG
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 创建 ViewModel 实例
            MainViewModel viewModel = new MainViewModel();

            // 绑定 DataContext，让窗口绑定到 ViewModel
            this.DataContext = viewModel;
        }


    }
}
