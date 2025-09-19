using GalaSoft.MvvmLight;
using KSEEG_Fversion_Lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Windows.Data.Json;
using Windows.Devices.Bluetooth;
using Windows.Media.PlayTo;
using static KSEEG_Fversion_Lib.BleManager;



namespace EEG.Tool
{
    public class KsEEG : ViewModelBase
    {
        private BleManager bleManager = BleManager.Instance;
        public BleManager BleManager
        {
            get { return bleManager; }
        }


        private BluetoothLEDevice selectDevice;//选中的设备
        public BluetoothLEDevice SelectDevice
        {
            get { return selectDevice; }
            set
            {
                selectDevice = value;

            }
        }

        private List<MyBleDevice> deviceList = new List<MyBleDevice>();//搜索到的设备列表
        public List<MyBleDevice> DeviceList
        {
            get { return deviceList; }
            set
            {
                deviceList = value;
                RaisePropertyChanged(() => DeviceList);
            }
        }

        public class MyBleDevice
        {
            public BluetoothLEDevice BleDevice { get; set; }
            public string Name => BleDevice.Name;
            public ulong BluetoothAddress => BleDevice.BluetoothAddress;
            public string DisplayInfo => $"{Name} ({BluetoothAddress})";
        }

        private bool connectState = false;//连接状态
        public bool ConnectState
        {
            get { return connectState; }
            set
            {
                connectState = value;
                RaisePropertyChanged(() => ConnectState);
            }
        }

        // 暴露事件给外部订阅
        public event Action<bool> onDisConnectSuccess;    // 断开连接成功
        public event Action<bool> onConnectFailure;       // 连接失败
        public event Action<bool> onServiceDiscoverySucceed; // 服务发现成功

        public event Action<BluetoothLEDevice> onDeviceFound; // 发现设备

        public event Action<JObject> onReceiveData;       // 接收原始数据
        public event Action<JObject> onReceiveFrequency;  // 接收频域数据
        public event Action<JObject> onReceiveEmotion;    // 接收情绪数据
        public event Action<JObject> onReceiveFeature;    // 接收特征数据
        public event Action<JObject> onReceivePPG;        // 接收PPG数据
        public event Action<JObject> onReceiveMPU;        // 接收MPU数据
        public event Action<JObject> onReceiveBlink;      // 接收眨眼数据
        public event Action<JObject> onReceiveGnash;      // 接收咬牙数据


        public KsEEG()
        {
            EEG_Init();
            SetupBleCallbacks();
            EEG_Scan();
        }

        public void EEG_Init()
        {
            // 初始化蓝牙设备，设置工作模式
            byte[] set = new byte[] { (byte)0b11111011, (byte)0b00000000 };
            
            BleManager.init(set);

        }

        public void EEG_Scan()
        {
            // 扫描设备
            BleManager.scanDevice();
            Debug.WriteLine("开始扫描设备");
            //MessageBox.Show("开始扫描设备");
        }



        private void SetupBleCallbacks()
        {
            // 断开连接成功
            bleManager.onDisConnectSuccess += () =>
            {
                onDisConnectSuccess?.Invoke(true);
            };

            // 连接失败
            bleManager.onConnectFailure += () =>
            {
                onConnectFailure?.Invoke(true);
            };

            // 服务发现成功
            bleManager.onServiceDiscoverySucceed += () =>
            {
                onServiceDiscoverySucceed?.Invoke(true);
            };

            // 发现设备
            bleManager.onDeviceFound += (BluetoothLEDevice device) =>
            {
                onDeviceFound?.Invoke(device);
            };

            // 数据相关的回调
            bleManager.onReceiveData += (JObject obj) =>
            {
                onReceiveData?.Invoke(obj);
            };

            bleManager.onReceiveFrequency += (JObject obj) =>
            {
                onReceiveFrequency?.Invoke(obj);
            };

            bleManager.onReceiveEmotion += (JObject obj) =>
            {
                onReceiveEmotion?.Invoke(obj);
            };

            bleManager.onReceiveFeature += (JObject obj) =>
            {
                onReceiveFeature?.Invoke(obj);
            };

            bleManager.onReceivePPG += (JObject obj) =>
            {
                onReceivePPG?.Invoke(obj);
            };

            bleManager.onReceiveMPU += (JObject obj) =>
            {
                onReceiveMPU?.Invoke(obj);
            };

            bleManager.onReceiveBlink += (JObject obj) =>
            {
                onReceiveBlink?.Invoke(obj);
            };

            bleManager.onReceiveGnash += (JObject obj) =>
            {
                onReceiveGnash?.Invoke(obj);
            };
        }



        private bool _isFilterEnable = true;
        public bool IsFilterEnable
            { get => _isFilterEnable; set => _isFilterEnable = value; }
        private bool _isNotchFilterEnable=true;
        public bool IsNotchFilterEnable
        { get => _isNotchFilterEnable; set => _isNotchFilterEnable = value; }
            private bool _is50Hz=true;
        public bool Is50Hz
            { get => _is50Hz; set => _is50Hz = value; }
        private bool _isOnlyHardware=true;
        public bool IsOnlyHardware
            { get => _isOnlyHardware; set => _isOnlyHardware = value; }
        private int _lightValue=2;
        public int LightValue
        { get=>_lightValue; set => _lightValue = value; }
        private int _magnifyValue=0;
        public int MagnifyValue
            { get => _magnifyValue; set => _magnifyValue = value; }
      




    }
}
