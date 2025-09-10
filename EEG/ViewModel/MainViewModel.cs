using EEG.Tool;
using EEG.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KSEEG_Fversion_Lib;
using NBitcoin.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Windows.Devices.Bluetooth;
using static EEG.Tool.KsEEG;
using static KSEEG_Fversion_Lib.BleManager;

namespace EEG.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        private KsEEG ksEEG = new KsEEG();
        public KsEEG KsEEG
        {
            get { return ksEEG; }
        }
        public MainViewModel()
        {




            ScanCommand = new RelayCommand(scanCommand);
            ConnectCommand = new RelayCommand(connectCommand);
            DisconnectCommand = new RelayCommand(disconnectCommand);
            SaveCommand=new RelayCommand(saveCommand);
            OpenSettingsCommand=new RelayCommand(openSettingCommand);
            DealBleCallback();


        }
        public void DealBleCallback()
        {
            // ---------------- 连接状态相关 ----------------
            KsEEG.onDisConnectSuccess += success =>
            {
                RunOnUI(() =>
                {
                    KsEEG.ConnectState = false;
                    Debug.WriteLine("已断开连接");
                });
            };

            KsEEG.onConnectFailure += failed =>
            {
                RunOnUI(() =>
                {
                    KsEEG.ConnectState = false;
                    Debug.WriteLine("连接失败");
                });
            };

            KsEEG.onServiceDiscoverySucceed += success =>
            {
                RunOnUI(() =>
                {
                    //KsEEG.ConnectState = true;
                    Debug.WriteLine("服务发现成功");
                });
            };

            // ---------------- 设备相关 ----------------
            KsEEG.onDeviceFound += device =>
            {

                RunOnUI(() =>
                {
                    Debug.WriteLine($"发现设备: {device.Name}, MAC: {device.BluetoothAddress}");
                    if (!ksEEG.DeviceList.Any(d => d.BluetoothAddress == device.BluetoothAddress))
                    {
                        MyBleDevice temp = new MyBleDevice() { BleDevice = device };
                        // 避免重复添加
                        ksEEG.DeviceList.Add(temp);
                        DeviceList_show.Add(temp);

                    }


                });
            };

            // ---------------- 数据相关 ----------------
            KsEEG.onReceiveData += json =>
            {
                RunOnUI(() =>
                {


                    BatteryLevel = (float)json["power"];
                    IsLeadoff = (bool)json["lead"];
                    Debug.WriteLine("收到原始数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveFrequency += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到频域数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveEmotion += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到情绪数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveFeature += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到特征数据: " + json.ToString());
                });
            };

            KsEEG.onReceivePPG += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到 PPG 数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveMPU += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到 MPU 数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveBlink += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到眨眼数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveGnash += json =>
            {
                RunOnUI(() =>
                {
                    Debug.WriteLine("收到咬牙数据: " + json.ToString());
                });
            };
        }



        // 工具方法：切回 UI 线程执行
        private void RunOnUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action(); // 已经是 UI 线程
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(action); // 切换到 UI 线程
            }
        }


        #region 属性绑定
        private ObservableCollection<MyBleDevice> _deviceList_show { get; set; } = new ObservableCollection<MyBleDevice>();
        public ObservableCollection<MyBleDevice> DeviceList_show
        {
            get { return _deviceList_show; }
            set
            {
                _deviceList_show = value;
                RaisePropertyChanged(() => DeviceList_show);
            }
        }



        private MyBleDevice _selectedDevice_show;
        public MyBleDevice SelectedDevice_show
        {
            get { return _selectedDevice_show; }
            set
            {
                _selectedDevice_show = value;
                RaisePropertyChanged(() => SelectedDevice_show);
            }
        }


        private string _bleImageSource = "/Picture/关闭蓝牙_turn-off-bluetooth.png"; // 默认断开图片
        public string BleImageSource
        {
            get { return _bleImageSource; }
            set
            {
                _bleImageSource = value;
                RaisePropertyChanged(() => BleImageSource);
            }
        }

        private string _batteryImageSource = "/Picture/电池-满电.png"; // 默认满电图片
        public string BatteryImageSource
        {
            get { return _batteryImageSource; }
            set
            {
                _batteryImageSource = value;
                RaisePropertyChanged(() => BatteryImageSource);
            }
        }
        private string _headImageSource = "/Picture/头部lead_of.png"; // 头部图片
        public string HeadImageSource
        {
            get { return _headImageSource; }
            set
            {
                _headImageSource = value;
                RaisePropertyChanged(() => HeadImageSource);
            }
        }

        private Brush _saveButtonBrush =
        (SolidColorBrush)(new BrushConverter().ConvertFrom("#CECECE"));
        public Brush SaveButtonBrush
        {
            get { return _saveButtonBrush; }
            set
            {
                _saveButtonBrush = value;
                RaisePropertyChanged(() => SaveButtonBrush);
            }
        }

        private string _saveButtonContent = "Save";
        public string SaveButtonContent
        {
            get { return _saveButtonContent; }
            set
            {
                _saveButtonContent = value;
                RaisePropertyChanged(() => SaveButtonContent);
            }
        }

        private bool _isSaveButtonChecked = false;
        public bool IsSaveButtonChecked
        {
            get { return _isSaveButtonChecked; }
            set
            {
                _isSaveButtonChecked = value;

                SaveButtonContent=_isSaveButtonChecked ? "Saving" : "Save";
                SaveButtonBrush = _isSaveButtonChecked ? (SolidColorBrush)(new BrushConverter().ConvertFrom("#4CAF50")) : (SolidColorBrush)(new BrushConverter().ConvertFrom("#CECECE"));

                RaisePropertyChanged(() => IsSaveButtonChecked);
            }
        }

        private float _batteryLevel = 100; // 默认满电
        public float BatteryLevel
        {
            get { return _batteryLevel; }
            set
            {
                _batteryLevel = value;
                RaisePropertyChanged(() => BatteryLevel);
                // 根据电量更新图片
                if (_batteryLevel >= 70)
                {

                    BatteryImageSource = "/Picture/电池-满电.png";
                }
                else if (_batteryLevel >= 40)
                {
                    BatteryImageSource = "/Picture/电池-中电量.png";
                }
                else
                {
                    BatteryImageSource = "/Picture/电池-没电.png";
                }
            }
        }

        private bool _isLeadoff = true; // 默认断开
        public bool IsLeadoff
        {
            get { return _isLeadoff; }
            set
            {
                _isLeadoff = value;
                RaisePropertyChanged(() => IsLeadoff);
                // 根据状态更新图片
                HeadImageSource = _isLeadoff ? "/Picture/头部lead_of.png" : "/Picture/头部.png";
            }
        }
        #endregion


        #region
        // 这里可以添加其他属性和命令，供界面绑定使用
        public RelayCommand ScanCommand { get; set; }
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand OpenSettingsCommand { get; }
        public void scanCommand()
        {
            KsEEG.EEG_Scan();

        }


        public void connectCommand()
        {

            if (SelectedDevice_show != null)
            {
                KsEEG.BleManager.connectDevice(SelectedDevice_show.BleDevice);

                // 连接成功，切换图片
                BleImageSource = "/Picture/蓝牙_bluetooth.png";
                ksEEG.ConnectState = true;
            }
            else
            {

                MessageBox.Show("Please select the device first", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }

        public void disconnectCommand()
        {
            if (SelectedDevice_show != null)
            {

                // 先保存要提示的内容
                string msg = $"Disconnect the device: {SelectedDevice_show.Name} ({SelectedDevice_show.BluetoothAddress})";
                BleImageSource="/Picture/关闭蓝牙_turn-off-bluetooth.png";
                // 断开连接
                KsEEG.BleManager.disconnectDevice();

                // 从列表移除
                ksEEG.DeviceList.Remove(SelectedDevice_show);
                DeviceList_show.Remove(SelectedDevice_show);
                ksEEG.ConnectState = false;
                // 弹窗提示
                MessageBox.Show(msg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        public void saveCommand()
        {
            if (ksEEG.ConnectState == true)
            {
                
                    ksEEG.BleManager.store();
                    IsSaveButtonChecked=!IsSaveButtonChecked;
                    if (IsSaveButtonChecked==true)
                    {
                        ksEEG.BleManager.store();
                    }
                    else
                    {
                        ksEEG.BleManager.stopStore();
                        MessageBox.Show("Data saved successfully", "Information",MessageBoxButton.OK, MessageBoxImage.Information);
                    }

            }
            else
            {
                MessageBox.Show("Please connect the device first", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        public void openSettingCommand()
        {

            //if(ksEEG.ConnectState==false)
            //{
                //MessageBox.Show("Please connect the device first", "Warning",
                //MessageBoxButton.OK, MessageBoxImage.Warning);
                //return;
            //}
            SettingsWindow settingsWindow = new SettingsWindow(ksEEG);
            settingsWindow.ShowDialog();
        }

        #endregion


    }
}
