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
        // 保存图表原始布局信息
        private Dictionary<PlotView, (int Row, int Column, int RowSpan, int ColumnSpan)> _originalLayouts = new Dictionary<PlotView, (int, int, int, int)>();
        // 当前放大的图表
        private PlotView _expandedPlot = null;
        public MainWindow()
        {
            InitializeComponent();
            // 创建 ViewModel 实例
            MainViewModel viewModel = new MainViewModel();

            // 绑定 DataContext，让窗口绑定到 ViewModel
            this.DataContext = viewModel;
        }

        private void PlotView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
