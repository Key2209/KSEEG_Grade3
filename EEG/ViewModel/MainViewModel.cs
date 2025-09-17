using EEG.Tool;
using EEG.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KSEEG_Fversion_Lib;
using NBitcoin.Logging;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Legends;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;


using static EEG.Tool.KsEEG;
using static EEG.Tool.OxyChart;
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
            SaveCommand = new RelayCommand(saveCommand);
            OpenSettingsCommand = new RelayCommand(openSettingCommand);

            InitChart();
            DealBleCallback();
            init_timer();

        }
        public void DealBleCallback()
        {
            // ---------------- 连接状态相关 ----------------
            KsEEG.onDisConnectSuccess += success =>
            {
                RunOnUI(() =>
                {
                    KsEEG.ConnectState = false;
                    BleImageSource = "/Picture/关闭蓝牙_turn-off-bluetooth.png";
                    HeadImageSource="/Picture/头部脱落2.png";
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

                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理

                    JArray channel_1 = (JArray)json["channel_1"];
                    JArray channel_2 = (JArray)json["channel_2"];
                    //Debug.WriteLine("原始数据： " + channel_1.ToString() + "-----" + channel_2.ToString());
                    RawDataChart.addPointArray(channel_1, channel_2);

                    //Debug.WriteLine("收到原始数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveFrequency += json =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                    double delta = (double)json["delta"];
                    double theta = (double)json["theta"];
                    double alpha = (double)json["alpha"];
                    double beta = (double)json["beta"];
                    double gamma = (double)json["gamma"];
                    BrainwaveChart.addPointNum(delta, theta, alpha, beta, gamma);

                    //Debug.WriteLine("收到频域数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveEmotion += json =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                    double emotion = (double)json["emotion"];
                    EmotionChart.addPointNum(emotion);
                    //Debug.WriteLine("收到情绪数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveFeature += json =>
            {
                RunOnUI(() =>
                {

                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                    // 处理特征数据
                    double stress = (double)json["tired"];
                    double relax = (double)json["relax"];
                    double focus = (double)json["attention"];
                    double mindful = (double)json["meditation"];
                    RelaxationChart.addPointNum(relax);
                    FocusChart.addPointNum(focus);
                    MeditationChart.addPointNum(mindful);
                    TiredChart.addPointNum(stress);

                    //Debug.WriteLine("收到特征数据: " + json.ToString());
                });
            };

            KsEEG.onReceivePPG += json =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                JArray red = (JArray)json["red"];
                JArray ired = (JArray)json["ired"];
                int spo2 = 0;
                int hr = 0;
                if (json.ContainsKey("spo2"))
                {
                    spo2 = (int)json["spo2"];
                    hr = (int)json["hr"];

                    }
                    _redTemp = (double)red.First();
                    _iredTemp = (double)ired.First();
                    _spo2Temp = spo2;
                    _hrTemp = hr;

                    //RedText= red.First().ToString();
                    //IredText=ired.First().ToString();

                    //Spo2Text=spo2.ToString();
                    //HrText= hr.ToString();

                });
            };

            KsEEG.onReceiveMPU += data =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return;
                    // 如果头部脱落，跳过数据处理


                    _accxTemp = ((double)data["acc_x"].First);
                    _accyTemp = ((double)data["acc_y"].First);
                    _acczTemp = ((double)data["acc_z"].First);
                    _gyroxTemp = ((double)data["gyro_x"].First);
                    _gyroyTemp = ((double)data["gyro_y"].First);
                    _gyrozTemp = ((double)data["gyro_z"].First);



                    //AccXText = ((double)data["acc_x"].First).ToString();
                    //AccYText=((double)data["acc_y"].First).ToString();
                    //AccZText=((double)data["acc_z"].First).ToString();
                    //GyroXText = ((double)data["gyro_x"].First).ToString();
                    //GyroYText = ((double)data["gyro_y"].First).ToString();
                    //GyroZText=((double)data["gyro_z"].First).ToString();


                    //Debug.WriteLine("收到 MPU 数据: " + data.ToString());
                });
            };

            KsEEG.onReceiveBlink += json =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                    JArray blink = (JArray)json["blink"];
                    BlinkChart.addPointArray(blink);
                    //Debug.WriteLine("收到眨眼数据: " + json.ToString());
                });
            };

            KsEEG.onReceiveGnash += json =>
            {
                RunOnUI(() =>
                {
                    if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                    JArray gnash = (JArray)json["gnash"];
                    GnashChart.addPointArray(gnash);
                    //Debug.WriteLine("收到咬牙数据: " + json.ToString());
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
        private string _headImageSource = "/Picture/头部脱落2.png"; // 头部图片
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

                SaveButtonContent = _isSaveButtonChecked ? "Saving" : "Save";
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
                HeadImageSource = _isLeadoff ? "/Picture/头部未脱落2.png" : "/Picture/头部脱落2.png";
            }
        }


        // ======================
        // 临时缓存变量
        // ======================

        private double _redTemp = 0.0;
        private double _iredTemp = 0.0;
        private int _spo2Temp = 0;
        private int _hrTemp = 0;
        private double _accxTemp = 0.0;
        private double _accyTemp = 0.0;
        private double _acczTemp = 0.0;
        private double _gyroxTemp = 0.0;
        private double _gyroyTemp = 0.0;
        private double _gyrozTemp = 0.0;
        // ======================
        // 定时器
        // ======================

        private  DispatcherTimer _updateTimer;
        private const int TimerIntervalMs = 500; // 每200毫秒更新一次UI，可以根据需要调整

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_updateTimer != null)
            {
                if (!IsLeadoff) return; // 如果头部脱落，跳过数据处理
                // 更新 UI 绑定属性
                RedText = _redTemp.ToString("F2"); // 保留两位小数
                IredText = _iredTemp.ToString("F2");
                Spo2Text = _spo2Temp.ToString();
                HrText = _hrTemp.ToString();
                AccXText = _accxTemp.ToString();
                AccYText = _accyTemp.ToString();
                AccZText = _acczTemp.ToString();
                GyroXText = _gyroxTemp.ToString();
                GyroYText = _gyroyTemp.ToString();
                GyroZText = _gyrozTemp.ToString();
            }
            // （可选）如果你希望在每次更新后执行某些操作，可以在这里添加
        }
        public void init_timer()
        {
            // 初始化定时器
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TimerIntervalMs)
            };
            _updateTimer.Tick += UpdateTimer_Tick;

            // 启动定时器
            _updateTimer.Start();
        }

        // 第一个 Border 中的传感器数据（可能是 PPG/血氧相关）
        private string _redText="None";
        private string _iredText = "None";
        private string _spo2Text = "None";
        private string _hrText = "None";

        // 第二个 Border 中的传感器数据（可能是加速度计/陀螺仪相关）
        private string _accXText = "None";
        private string _accYText = "None";
        private string _accZText = "None";
        private string _gyroXText = "None";  // 注意：可能是 "Gyro X" 的拼写错误（陀螺仪 X 轴）
        private string _gyroYText = "None";  // 同理，可能是 "Gyro Y"
        private string _gyroZText = "None";  // 同理，可能是 "Gyro Z"


        // 第一个 Border：PPG/血氧相关数据
        public string RedText
        {
            get => _redText;
            set
            {
                _redText = value;
                RaisePropertyChanged();
            }
        }

        public string IredText
        {
            get => _iredText;
            set
            {
                _iredText = value;
                RaisePropertyChanged();
            }
        }

        public string Spo2Text
        {
            get => _spo2Text;
            set
            {
                _spo2Text = value;
                RaisePropertyChanged();
            }
        }

        public string HrText
        {
            get => _hrText;
            set
            {
                _hrText = value;
                RaisePropertyChanged();
            }
        }


        // 第二个 Border：加速度计/陀螺仪相关数据
        public string AccXText
        {
            get => _accXText;
            set
            {
                _accXText = value;
                RaisePropertyChanged();
            }
        }

        public string AccYText
        {
            get => _accYText;
            set
            {
                _accYText = value;
                RaisePropertyChanged();
            }
        }

        public string AccZText
        {
            get => _accZText;
            set
            {
                _accZText = value;
                RaisePropertyChanged();
            }
        }

        public string GyroXText  // 建议修正为 GyroXText（更符合命名规范）
        {
            get => _gyroXText;
            set
            {
                _gyroXText = value;
                RaisePropertyChanged();
            }
        }

        public string GyroYText  // 建议修正为 GyroYText
        {
            get => _gyroYText;
            set
            {
                _gyroYText = value;
                RaisePropertyChanged();
            }
        }

        public string GyroZText  // 建议修正为 GyroZText
        {
            get => _gyroZText;
            set
            {
                _gyroZText = value;
                RaisePropertyChanged();
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
                ksEEG.BleManager.scanStop();
                KsEEG.BleManager.connectDevice(SelectedDevice_show.BleDevice);

                // 连接成功，切换图片
                BleImageSource = "/Picture/蓝牙_bluetooth.png";
                ksEEG.ConnectState = true;
                ksEEG.BleManager.setNotchFilterEnable(true);
                ksEEG.BleManager.setFilterEnable(true);
                ksEEG.BleManager.setLight(0);
                ksEEG.BleManager.setMagnify(3);

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
                BleImageSource = "/Picture/关闭蓝牙_turn-off-bluetooth.png";
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

                
                IsSaveButtonChecked = !IsSaveButtonChecked;
                if (IsSaveButtonChecked == true)
                {
                    ksEEG.BleManager.store();
                }
                else
                {
                    ksEEG.BleManager.stopStore();
                    var r=MessageBox.Show("Data saved successfully,Do you want to open the file path?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if(r==MessageBoxResult.Yes)
                    {
                        try
                        {
                            // 替换为你的目标路径
                            string targetPath = @"D:\KSEEG";
                            Process.Start("explorer.exe", targetPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"打开失败: {ex.Message}");
                        }
                    }
                    


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
            SettingsWindow settingsWindow = new SettingsWindow( this,ksEEG);
            settingsWindow.ShowDialog();
        }

        #endregion

        #region 图表相关


        private OxyChart _RawDataChart;
        public OxyChart RawDataChart
        {
            get { return _RawDataChart; }
            set
            {
                _RawDataChart = value;
                RaisePropertyChanged(() => RawDataChart);
            }
        }
        private PlotModel _RawDataPlotModel;
        public PlotModel RawDataPlotModel
        {
            get => _RawDataPlotModel;
            set
            {
                _RawDataPlotModel = value;
                RaisePropertyChanged(() => RawDataPlotModel);
            }
        }

        private OxyChart _BrainwaveChart;
        public OxyChart BrainwaveChart
        {
            get { return _BrainwaveChart; }
            set
            {
                _BrainwaveChart = value;
                RaisePropertyChanged(() => BrainwaveChart);
            }
        }
        private PlotModel _BrainwavePlotModel;
        public PlotModel BrainwavePlotModel
        {
            get => _BrainwavePlotModel;
            set
            {
                _BrainwavePlotModel = value;
                RaisePropertyChanged(() => BrainwavePlotModel);
            }
        }

        private OxyChart _FocusChart;
        public OxyChart FocusChart
        {
            get { return _FocusChart; }
            set
            {
                _FocusChart = value;
                RaisePropertyChanged(() => FocusChart);
            }
        }
        private PlotModel _FocusPlotModel;
        public PlotModel FocusPlotModel
        {
            get => _FocusPlotModel;
            set
            {
                _FocusPlotModel = value;
                RaisePropertyChanged(() => FocusPlotModel);
            }
        }


        private OxyChart _MeditationChart;
        public OxyChart MeditationChart
        {
            get { return _MeditationChart; }
            set
            {
                _MeditationChart = value;
                RaisePropertyChanged(() => MeditationChart);
            }
        }
        private PlotModel _MeditationPlotModel;
        public PlotModel MeditationPlotModel
        {
            get => _MeditationPlotModel;
            set
            {
                _MeditationPlotModel = value;
                RaisePropertyChanged(() => MeditationPlotModel);
            }
        }

        private OxyChart _RelaxationChart;
        public OxyChart RelaxationChart
        {
            get { return _RelaxationChart; }
            set
            {
                _RelaxationChart = value;
                RaisePropertyChanged(() => RelaxationChart);
            }
        }
        private PlotModel _RelaxationPlotModel;
        public PlotModel RelaxationPlotModel
        {
            get => _RelaxationPlotModel;
            set
            {
                _RelaxationPlotModel = value;
                RaisePropertyChanged(() => RelaxationPlotModel);
            }
        }

        private OxyChart _TiredChart;
        public OxyChart TiredChart
        {
            get { return _TiredChart; }
            set
            {
                _TiredChart = value;
                RaisePropertyChanged(() => TiredChart);
            }
        }
        private PlotModel _TiredPlotModel;
        public PlotModel TiredPlotModel
        {
            get => _TiredPlotModel;
            set
            {
                _TiredPlotModel = value;
                RaisePropertyChanged(() => TiredPlotModel);
            }
        }

        private OxyChart _EmotionChart;
        public OxyChart EmotionChart
        {
            get { return _EmotionChart; }
            set
            {
                _EmotionChart = value;
                RaisePropertyChanged(() => EmotionChart);
            }
        }
        private PlotModel _EmotionPlotModel;
        public PlotModel EmotionPlotModel
        {
            get => _EmotionPlotModel;
            set
            {
                _EmotionPlotModel = value;
                RaisePropertyChanged(() => EmotionPlotModel);
            }
        }

        private OxyChart _Blink;
        public OxyChart BlinkChart
        {
            get { return _Blink; }
            set
            {
                _Blink = value;
                RaisePropertyChanged(() => BlinkChart);
            }
        }
        private PlotModel _BlinkPlotModel;
        public PlotModel BlinkPlotModel
        {
            get => _BlinkPlotModel;
            set
            {
                _BlinkPlotModel = value;
                RaisePropertyChanged(() => BlinkPlotModel);
            }
        }

        private OxyChart _Gnash;
        public OxyChart GnashChart
        {
            get { return _Gnash; }
            set
            {
                _Gnash = value;
                RaisePropertyChanged(() => GnashChart);
            }
        }
        private PlotModel _GnashPlotModel;
        public PlotModel GnashPlotModel
        {
            get => _GnashPlotModel;
            set
            {
                _GnashPlotModel = value;
                RaisePropertyChanged(() => GnashPlotModel);
            }
        }



        public void InitChart()
        {



            RawDataChart = new OxyChart(
                title: "Raw Signal",
                xSize: 2000, // X 轴窗口长度
                ySize: 2, // Y 轴范围
                lineCount: 2,
                legendTitles: new[] { "CHL1(AF8)", "CHL2(AF7)" },
                lineColors: new[] { OxyColors.Red, OxyColors.Green },
                legendPosition: LegendPosition.TopLeft
            );
            RawDataPlotModel = RawDataChart.init();

            BrainwaveChart = new OxyChart(
                title: "Frequency Spectrum",
                xSize: 200, // X 轴窗口长度
                ySize: 100, // Y 轴范围
                lineCount: 5,
                legendTitles: new[] { "Delta", "Theta", "Alpha", "Beta", "Gamma" },
                lineColors: new[] { OxyColors.Red, OxyColors.Orange, OxyColors.Yellow, OxyColors.Green, OxyColors.Blue },
                legendPosition: LegendPosition.TopLeft
            );

            BrainwavePlotModel = BrainwaveChart.init();
            FocusChart = new OxyChart(
                title: "Focus Index",
                xSize: 200, // X 轴窗口长度
                ySize: 100, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Focus" },
                lineColors: new[] { OxyColors.Red },
                legendPosition: LegendPosition.TopLeft
            );

            FocusPlotModel = FocusChart.init();
            MeditationChart = new OxyChart(
                title: "Meditation Index",
                xSize: 100, // X 轴窗口长度
                ySize: 100, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Meditation" },
                lineColors: new[] { OxyColors.Green },
                legendPosition: LegendPosition.TopLeft
            );
            MeditationPlotModel = MeditationChart.init();
            RelaxationChart = new OxyChart(
                title: "Relax Index",
                xSize: 100, // X 轴窗口长度
                ySize: 100, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Relax" },
                lineColors: new[] { OxyColors.Blue },
                legendPosition: LegendPosition.TopLeft
            );
            RelaxationPlotModel = RelaxationChart.init();

            TiredChart = new OxyChart(
                title: "Fatigue Index",
                xSize: 100, // X 轴窗口长度
                ySize: 100, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Fatigue" },
                lineColors: new[] { OxyColors.Purple },
                legendPosition: LegendPosition.TopLeft
            );
            TiredPlotModel = TiredChart.init();
            EmotionChart = new OxyChart(
                title: "Emotion Index",
                xSize: 100, // X 轴窗口长度
                ySize: 2, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Emotion" },
                lineColors: new[] { OxyColors.Orange },
                legendPosition: LegendPosition.TopLeft
            );
            EmotionPlotModel = EmotionChart.init();
            EmotionPlotModel.Axes[1].Minimum = -2;


            BlinkChart = new OxyChart(
                title: "Blink Signal",
                xSize: 100, // X 轴窗口长度
                ySize: 2, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Blink" },
                lineColors: new[] { OxyColors.Brown },
                legendPosition: LegendPosition.TopLeft
            );
            BlinkPlotModel = BlinkChart.init();
            BlinkPlotModel.Axes[1].Minimum = -1;
            GnashChart = new OxyChart(
                title: "Gnash Signal",
                xSize: 100, // X 轴窗口长度
                ySize: 2, // Y 轴范围
                lineCount: 1,
                legendTitles: new[] { "Gnash" },
                lineColors: new[] { OxyColors.Gray },
                legendPosition: LegendPosition.TopLeft
            );
            GnashPlotModel = GnashChart.init();
            GnashPlotModel.Axes[1].Minimum = -1;
        }

        #endregion

        public void OnWindowClosing()
        {
            // ✅ 在这里编写你的关闭前需要做的所有逻辑
            // 比如：断开蓝牙、停止线程、保存数据、释放资源等

                ksEEG.BleManager.disconnectDevice();
                Debug.WriteLine("窗口关闭，已断开蓝牙连接");
            // 🎯 注意：这里是你业务逻辑的关键，可以自由扩展
        }



        #region
        private Visibility _max30102Show = Visibility.Collapsed;
        public Visibility MAX30102_show
        {
            get => _max30102Show;
            set
            {
                _max30102Show = value;
                RaisePropertyChanged(nameof(MAX30102_show));
            }
        }
        private bool _isMax30102Visible=false;
        public bool IsMax30102Visible
        {
            get => _isMax30102Visible;
            set
            {
                if (_isMax30102Visible != value)
                {
                    _isMax30102Visible = value;
                    RaisePropertyChanged(nameof(IsMax30102Visible));

                    // 同步更新 Visibility
                    MAX30102_show = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }


        private Visibility _MPU6050Show = Visibility.Collapsed;
        public Visibility MPU6050_show
        {
            get => _MPU6050Show;
            set
            {
                _MPU6050Show = value;
                RaisePropertyChanged(nameof(MPU6050_show));
            }
        }
        private bool _isMPU6050Visible=false;
        public bool IsMPU6050Visible
        {
            get => _isMPU6050Visible;
            set
            {
                if (_isMPU6050Visible != value)
                {
                    _isMPU6050Visible = value;
                    RaisePropertyChanged(nameof(IsMPU6050Visible));

                    // 同步更新 Visibility
                    MPU6050_show = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        #endregion
    }
}
