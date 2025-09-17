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
        public bool mpu6050 { get; set; }
        public bool max30102 { get; set; }
        public SettingsWindow(MainViewModel vm,KsEEG ksEEG)
        {
            this.ksEEG = ksEEG;
            //this.mpu6050 = mpu6050;
            //this.max30102=max30102;
            InitializeComponent();
            this.DataContext = vm;
            //MAX30102_Toggle.IsChecked=this.max30102;
            //MPU6050_Toggle.IsChecked=this.mpu6050;

            BP_FilterToggle.IsChecked = ksEEG.IsFilterEnable;
            Notch_FilterToggle.IsChecked = ksEEG.IsNotchFilterEnable;
            PLI_Combobox.SelectedIndex = ksEEG.Is50Hz ? 0 : 1;
            Leadoff_Combobox.SelectedIndex = ksEEG.IsOnlyHardware ? 0 : 1;
            LightCombobox.SelectedIndex = ksEEG.LightValue;
            SignalCombobox.SelectedIndex = ksEEG.MagnifyValue;

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

            ksEEG.IsFilterEnable = true;
            Debug.WriteLine("setFilterEnable=ture");
            //MessageBox.Show("已开启滤波器，建议佩戴设备后再开启");
        }

        private void BP_UnChecked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setFilterEnable(false);

            ksEEG.IsFilterEnable = false;
            Debug.WriteLine("setFilterEnable=false");
            //MessageBox.Show("已关闭滤波器，可能会有较多噪声");
        }

        private void Notch_Checked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setNotchFilterEnable(true);

            ksEEG.IsNotchFilterEnable = true ;
            Debug.WriteLine("setNotchFilterEnable=true");
            //MessageBox.Show("已开启工频滤波器，建议佩戴设备后再开启");
        }

        private void Notch_UnChecked(object sender, RoutedEventArgs e)
        {
            ksEEG.BleManager.setNotchFilterEnable(false);

            ksEEG.IsNotchFilterEnable= false ;
            Debug.WriteLine("setNotchFilterEnable=false");
            //MessageBox.Show("已关闭工频滤波器，可能会有较多噪声");
        }

        private void PLI_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PLI_Combobox.SelectedIndex == 0)
            {
                ksEEG.BleManager.setIs50Hz(true);
                ksEEG.Is50Hz=true;
                Debug.WriteLine("已设置工频滤波器为50Hz");
            }
            else if (PLI_Combobox.SelectedIndex == 1)
            {
                ksEEG.BleManager.setIs50Hz(false);
                ksEEG.Is50Hz=false;
                Debug.WriteLine("已设置工频滤波器为60Hz");
            }
        }

        private void Leadoff_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Leadoff_Combobox.SelectedIndex == 0)
            {
                ksEEG.BleManager.setOnlyHardware(true);

                ksEEG.IsOnlyHardware=true;
                Debug.WriteLine("setOnlyHardware=true");
                //MessageBox.Show("已开启只有硬件检测，建议佩戴设备后再开启");
            }
            else if (Leadoff_Combobox.SelectedIndex == 1)
            {
                ksEEG.BleManager.setOnlyHardware(false);

                ksEEG.IsOnlyHardware=false;
                Debug.WriteLine("setNotchFilterEnable=false");
                //MessageBox.Show("已开启硬件和软件检测，可能会有误报");

            }
        }

        private void Light_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (LightCombobox.SelectedIndex)
            {
                case 0:
                    ksEEG.BleManager.setLight(3);
                    ksEEG.LightValue = 0;
                    Debug.WriteLine("setLight=3");
                    break;
                case 1:
                    ksEEG.BleManager.setLight(2);
                    ksEEG.LightValue = 1;
                    Debug.WriteLine("setLight=2");
                    break;
                case 2:
                    ksEEG.BleManager.setLight(0);
                    ksEEG.LightValue = 2;
                    Debug.WriteLine("setLight=0");
                    break;
                case 3:
                    ksEEG.BleManager.setLight(1);
                    ksEEG.LightValue = 3;
                    Debug.WriteLine("setLight=1");
                    break;

            }

        }

        private void Signal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ksEEG.BleManager.setMagnify(SignalCombobox.SelectedIndex + 3);
            ksEEG.MagnifyValue=SignalCombobox.SelectedIndex;
            Debug.WriteLine($"已设置信号增益为{SignalCombobox.SelectedIndex + 3}");
        }

        private void MAX30102_Checked(object sender, RoutedEventArgs e)
        {
            max30102 = true;
        }

        private void MAX30102_UnChecked(object sender, RoutedEventArgs e)
        {
            max30102 = false;
        }

        private void MPU6050_Checked(object sender, RoutedEventArgs e)
        {
            mpu6050 = true;
        }

        private void MPU6050_UnChecked(object sender, RoutedEventArgs e)
        {
            mpu6050 = false;
        }
    }
}
