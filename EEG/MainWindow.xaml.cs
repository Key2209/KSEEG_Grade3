using EEG.ViewModel;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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


            _overlayGrid = OverlayGrid; // 绑定 XAML OverlayGrid
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.OnWindowClosing();
        }


        private PlotView _currentZoomedPlot = null;
        private Grid _overlayGrid;


        private void PlotView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var plot = sender as PlotView;
            if (plot == null) return;

            if (_currentZoomedPlot == null)
            {
                // 放大
                _currentZoomedPlot = plot;

                // 从原父容器移除
                var parent = plot.Parent as Panel;
                if (parent != null) parent.Children.Remove(plot);

                // 添加到 OverlayGrid
                _overlayGrid.Children.Add(plot);
                _overlayGrid.Visibility = Visibility.Visible;

                // 设置铺满
                plot.HorizontalAlignment = HorizontalAlignment.Stretch;
                plot.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                // 复原
                var parentGrid = MainGrid; // 这里所有 Plot 都在 MainGrid 或子 Grid，可以根据 Tag 找位置

                _overlayGrid.Children.Remove(_currentZoomedPlot);

                // 根据 Tag 决定返回哪个父容器
                switch (_currentZoomedPlot.Tag.ToString())
                {
                    case "RawData":
                        parentGrid.Children.Add(_currentZoomedPlot);
                        Grid.SetRow(_currentZoomedPlot, 0);
                        break;
                    case "Brainwave":
                        parentGrid.Children.Add(_currentZoomedPlot);
                        Grid.SetRow(_currentZoomedPlot, 1);
                        break;
                    case "Focus":
                        var grid2 = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 2);
                        grid2.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 0);
                        break;
                    case "Meditation":
                        var grid2b = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 2);
                        grid2b.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 1);
                        break;
                    case "Relaxation":
                        var grid3 = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                        grid3.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 0);
                        break;
                    case "Tired":
                        var grid3b = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 3);
                        grid3b.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 1);
                        break;
                    case "Emotion":
                        var grid4 = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 4);
                        grid4.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 0);
                        break;
                    case "Blink":
                        var grid4b = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 4);
                        grid4b.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 1);
                        break;
                    case "Gnash":
                        var grid4c = parentGrid.Children.OfType<Grid>().FirstOrDefault(g => Grid.GetRow(g) == 4);
                        grid4c.Children.Add(_currentZoomedPlot);
                        Grid.SetColumn(_currentZoomedPlot, 2);
                        break;
                }

                _overlayGrid.Visibility = Visibility.Collapsed;
                _currentZoomedPlot = null;
            }
        }

        // 遍历 Grid 所有子控件
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }







    }
}
