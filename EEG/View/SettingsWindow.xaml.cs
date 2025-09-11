using EEG.Tool;
using EEG.ViewModel;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace EEG.View
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public KsEEG ksEEG { get; }
        public string FirmwareVersion { get; set; }
        public SettingsWindow(KsEEG ksEEG)
        {this.ksEEG = ksEEG;
            
            InitializeComponent();
            string version = ksEEG.BleManager.getVersion(); // 假设返回 "vision:1.2.3"
            int index = version.IndexOf(':');               // 找到冒号的位置
            if (index >= 0 && index < version.Length - 1)
            {
                VersionTextBlock.Text = version.Substring(index + 1); // 截取冒号后的内容
            }
            else
            {
                VersionTextBlock.Text = version; // 如果没有冒号，直接显示原文
            }
            //this.ksEEG.BleManager.scanDevice();
            //MessageBox.Show("开始扫描设备");
            //SettingsViewModel viewModel = new SettingsViewModel(ksEEG);
            //this.DataContext = viewModel;

        }

        private void BP_Checked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setFilterEnable(true);
            //MessageBox.Show("已开启滤波器，建议佩戴设备后再开启");
        }

        private void BP_UnChecked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setFilterEnable(false);
            //MessageBox.Show("已关闭滤波器，可能会有较多噪声");
        }

        private void Notch_Checked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setNotchFilterEnable(true);

            //MessageBox.Show("已开启工频滤波器，建议佩戴设备后再开启");
        }

        private void Notch_UnChecked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setNotchFilterEnable(false);
            //MessageBox.Show("已关闭工频滤波器，可能会有较多噪声");
        }

        private void PLI_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PLI_Combobox.SelectedIndex == 0)
            {
                ksEEG.BleManager.setIs50Hz(true);
                Debug.WriteLine("已设置工频滤波器为50Hz");
            }
            else if(PLI_Combobox.SelectedIndex == 1)
            {
                ksEEG.BleManager.setIs50Hz(false);
                Debug.WriteLine("已设置工频滤波器为60Hz");
            }
        }

        private void Leadoff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Leadoff_Combobox.SelectedIndex == 0)
            {
                ksEEG.BleManager.setOnlyHardware(true);
                //MessageBox.Show("已开启只有硬件检测，建议佩戴设备后再开启");
            }
            else if(Leadoff_Combobox.SelectedIndex == 1)
            {
                ksEEG.BleManager.setOnlyHardware(false);
                //MessageBox.Show("已开启硬件和软件检测，可能会有误报");

            }
        }

        private void Light_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(LightCombobox.SelectedIndex)
            {
                case 0:
                    ksEEG.BleManager.setLight(3);
                    break;
                case 1:
                    ksEEG.BleManager.setLight(2);
                    break;
                case 2:
                    ksEEG.BleManager.setLight(0);
                    break;
                case 3:
                    ksEEG.BleManager.setLight(1);
                    break;

            }
            
        }

        private void Signal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ksEEG.BleManager.setMagnify(SignalCombobox.SelectedIndex + 3);
            Debug.WriteLine($"已设置信号增益为{SignalCombobox.SelectedIndex + 3}");
        }
    }
}
