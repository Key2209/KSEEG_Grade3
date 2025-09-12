using EEG.ViewModel;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace EEG
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            // 创建 ViewModel 实例
            viewModel = new MainViewModel();

            // 绑定 DataContext，让窗口绑定到 ViewModel
            this.DataContext = viewModel;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.OnWindowClosing();
        }

        private void PlotView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
