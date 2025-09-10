using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace EEG.Tools
{
    /// <summary>
    /// 基于OxyPlot实现的动态滚动图表控件
    /// </summary>
    /// 
    public class ChartConfig
    {
        public string Title { get; set; } = "实时数据图表"; // 图表标题
        public int MaxDataPoints { get; set; } = 200; // 最大数据点
        public int lineCount { get; set; }= 1; // 需要绘制的线条数量
        public float lineThickness { get; set; } = 2f; // 线条粗细
        public AxisConfig XAxis { get; set; } = new AxisConfig(); // X轴配置
        public AxisConfig YAxis { get; set;} = new AxisConfig();
        public Legend legend { get; set; } = new Legend
        {
            LegendSize = new OxySize(200, 10), // 图例大小
            LegendPlacement = LegendPlacement.Outside,
            LegendPosition = LegendPosition.TopLeft, // 图例位置
            LegendOrientation = LegendOrientation.Horizontal, // 图例垂直排列
            TextColor = OxyColors.White, // 图例文本颜色为白色
            LegendTitle = "数据曲线", // 图例标题
            LegendTextColor = OxyColors.White, // 图例文本颜色
            LegendTitleColor = OxyColors.White, // 图例标题颜色
            LegendFontSize = 8, // 图例字体大小
            IsLegendVisible = false, // 显示图例
            
            LegendSymbolLength = 5, // 减小符号长度
            LegendMargin = 1, // 减小边距
            LegendPadding = 1 // 减小内边距
        };
        //颜色配置
        //public OxyColor BackgroundColor { get; set; } = OxyColors.Red;
        public OxyColor BackgroundColor { get; set; } = OxyColor.Parse("#2E2E2E");
        public OxyColor PlotAreaBorderColor { get; set; } = OxyColors.Transparent; // 图表边框颜色
        
        public OxyColor TitleColor { get; set; } = OxyColors.White;
 
        public OxyColor LegendTextColor { get; set; } = OxyColors.White; // 图例文本颜色
        public OxyColor LegendBorderColor { get;set; } = OxyColors.Transparent; // 图例边框颜色
        public OxyColor LegendBorderWidth { get; set; } = OxyColors.Transparent; // 图例边框宽度
        public OxyColor LegendColor { get;set; } = OxyColors.Transparent; // 图例背景颜色

        public string[] lendendTitles; // 图例标题数组，长度应与lineCount一致



        public TwoColorLineSeriesConfig TwoColorLineSeries { get; set; } = new TwoColorLineSeriesConfig(); // 双颜色线条配置
    }
    public class TwoColorLineSeriesConfig
    {
        public bool IsEnabled { get; set; } = false; // 是否启用双颜色线条
        public OxyColor Color1 { get; set; } = OxyColors.Green; // 第一种颜色
        public OxyColor Color2 { get; set; } = OxyColors.Red;   // 第二种颜色
        public double Threshold { get; set; } = 0.5;            // 阈值，用于切换颜色
        public double StrokeThickness { get; set; } = 2.0;      // 线条粗细
    }


    public class AxisConfig
    {
        public float Max { get; set; } = 360; // X轴最大值
        public float Min { get; set; } = 0;   // X轴最小值
        public float MajorStep {get; set; } = 20; // X轴主刻度间隔
        public float lineThickness { get; set; } = 0.5f; // X轴线条粗细
  
        public string title { get; set; } = "数值";
        public LineStyle MajorGridlineStyle { get; set; } = LineStyle.Dash; // Y轴线条样式
        public LineStyle AxislineStyle { get; set; } = LineStyle.Solid; // Y轴线条样式
        public OxyColor TextColor { get; set; } = OxyColors.White; 

        public bool IsAxisVisible { get; set; } = false; // 是否显示轴线



    }
    internal class OxyChart
    {
        // 图表模型-核心对象
        private PlotModel plotModel;
        private PlotController plotCtrl; // 控制器（可选，用于交互）
        // 存储所有折线系列的集合
        private List<LineSeries> lineSeriesList = new List<LineSeries>();

        private ChartConfig config;
        // 时间/数据点计数器（用于X轴坐标）
        private int timeCounter = 0;
        // 预定义的线条颜色数组（最多支持5条线）
        private OxyColor[] colors = {
            OxyColors.Green,
            OxyColors.GreenYellow,
            OxyColors.Honeydew,
            OxyColors.HotPink,
            OxyColors.Ivory,
            OxyColors.Automatic,
            
        };
        //private string[] lendendTitles = {
        //    "delta",
        //    "theta",
        //    "alpha",
        //    "beta",
        //    "gamma",
        //    "6"
        //};
        // X轴显示窗口长度（最大显示点数）
        private float max_x;
        // Y轴最大值
        private float max_y;
        //// 需要绘制的线条数量
        private int lineCount;
        //private int MajorStepx;
        //private int MajorStepy;

        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="title">图表标题（当前未使用）</param>
        /// <param name="x_size">X轴显示窗口长度（最大显示点数）</param>
        /// <param name="y_size">Y轴最大值</param>
        /// <param name="lineCount">需要绘制的线条数量</param>

        public OxyChart(ChartConfig config)
        {
            plotModel = new PlotModel();
            plotCtrl = new PlotController(); // 初始化控制器（可选）
            this.config = config;
            lineCount=config.lineCount; // 获取需要绘制的线条数量
            max_x = config.MaxDataPoints; // 设置X轴显示窗口长度
            max_y = config.MaxDataPoints; // 设置Y轴最大值

        }
        public PlotController PlotController()
        {
            plotCtrl.UnbindMouseWheel(); // 禁用鼠标滚轮缩放（如果不需要缩放功能）
            return plotCtrl;
        }
        /// <summary>
        /// 初始化图表配置
        /// </summary>
        /// <returns>配置完成的PlotModel对象</returns>
        public PlotModel init()
        {
            plotModel.Title = config.Title; // 设置图表标题
            //plotModel.TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea;
            // 设置图表背景颜色（可选）
            plotModel.Background = config.BackgroundColor;
            plotModel.PlotAreaBorderColor = config.PlotAreaBorderColor; // 设置图表边框颜色为透明
            //plotModel.PlotAreaBorderColor = OxyColors.Black; // 设置图表边框颜色为透明
            plotModel.PlotAreaBorderThickness = new OxyThickness(1); // 设置图表边框颜色和厚度
            
            plotModel.TextColor = config.BackgroundColor; // 设置文本颜色为白色
            plotModel.TitleColor = config.TitleColor;// 设置标题颜色为白色
                                                     // Replace the following line:
                                                     // plotModel.ZoomAllAxes = false; // 禁用缩放功能
                                                     plotModel.Legends.Add(config.legend);
            // Replace the following line:
            // plotModel.ZoomAllAxes = true; // 启用缩放功能

            // With the correct method call:



            //plotModel.Legends.Add(new Legend
            //{
            //    LegendSize = new OxySize(200, 25), // 图例大小
            //    LegendPlacement = LegendPlacement.Outside,
            //    LegendPosition = LegendPosition.TopCenter, // 图例位置
            //    LegendOrientation = LegendOrientation.Vertical, // 图例垂直排列

            //    TextColor = OxyColors.White, // 图例文本颜色为白色
            //    LegendTitle = "数据曲线", // 图例标题
            //    IsLegendVisible = true // 显示图例
            //});
            //plotModel.PlotMargins = new OxyThickness(0);

            //plotModel.PlotAreaBackground = OxyColor.Parse("#000000"); // 设置绘图区背景颜色为透明


            // 配置X轴
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //PositionAtZeroCrossing = true,
                //MajorGridlineStyle =LineStyle.None,
                Title = "采样点",
                Minimum = config.XAxis.Min,            // 初始显示范围起点
                Maximum = config.XAxis.Max,        // 初始显示范围终点
                MajorGridlineColor = config.XAxis.TextColor,
                TextColor = config.XAxis.TextColor, // 
                //MajorGridlineStyle = config.XAxis.MajorGridlineStyle, // 设置为虚线
                //MajorGridlineColor = config.XAxis.TextColor,
                MajorGridlineThickness = config.XAxis.lineThickness, // 设置虚线的粗细
                AxislineColor = config.XAxis.TextColor, // 设置X轴线条颜色
                AxislineStyle = config.XAxis.MajorGridlineStyle, // 设置X轴线条样式
                TicklineColor = config.XAxis.TextColor, // 设置刻度线颜色
                //AxislineStyle=LineStyle.Solid,
                //MajorGridlineThickness=config.XAxis.lineThickness,
                //AxislineColor=OxyColors.White,
                //TicklineColor=OxyColors.White,




                //IsAxisVisible = config.XAxis.IsAxisVisible, // 隐藏X轴

            });


            
            // 配置Y轴
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = config.YAxis.title,
                Minimum = config.YAxis.Min,            // 初始显示范围起点
                Maximum = config.YAxis.Max,        // 初始显示范围终点
                
                //FontSize = 12, // 设置字体大小

                MajorStep = config.YAxis.MajorStep,       // 主刻度间隔
                AxislineStyle = config.YAxis.AxislineStyle,
                TextColor =config.YAxis.TextColor, // 设置Y轴文本颜色
                //MajorGridlineStyle = config.YAxis.MajorGridlineStyle, // 设置为虚线
                MajorGridlineColor = config.YAxis.TextColor,
                MajorGridlineThickness = config.YAxis.lineThickness, // 设置虚线的粗细
                AxislineColor = config.YAxis.TextColor, // 设置Y轴线条颜色
                TicklineColor = config.YAxis.TextColor, // 设置刻度线颜色

                MajorGridlineStyle = LineStyle.Dash,
                //MinorGridlineStyle = LineStyle.Dot,
                //MinorGridlineThickness=1, // 设置次刻度线粗细
                //AxislineStyle = LineStyle.Dash, // 设置Y轴线条样式为实线
                //MajorTickSize = 0, // 设置主刻度线长度为0
            });

            // 创建指定数量的折线系列
            if (config.TwoColorLineSeries.IsEnabled == false)
            {
            for (int i = 0; i < lineCount; i++)
            {
                // 创建折线配置
                LineSeries realTimeSeries = new LineSeries
                {
                    //Title = $"曲线{i + 1}", // 设置曲线标题
                    //Title= lendendTitles[i], // 设置曲线标题

                    Title = (config.lendendTitles != null)
                ? config.lendendTitles[i]
                : $"曲线{i + 1}",

                    Color = colors[i],               // 使用预定义颜色（注意i不能超过colors长度）
                    StrokeThickness = config.lineThickness,             // 线条粗细
                    MarkerType = MarkerType.None,    // 禁用数据点标记（提升性能）
                    Decimator = Decimator.Decimate,  // 启用数据缩减算法（提升渲染性能）
                    RenderInLegend = true           // 不在图例中显示
                };

                lineSeriesList.Add(realTimeSeries);  // 添加到管理列表
                plotModel.Series.Add(realTimeSeries); // 添加到图表模型
            }

            }
            else
            {                 // 创建双颜色线条系列
                LineSeries twoColorSeries = new TwoColorLineSeries
                {
                    Color = config.TwoColorLineSeries.Color1,
                    StrokeThickness = config.lineThickness,             // 线条粗细
                    MarkerType = MarkerType.None,    // 禁用数据点标记（提升性能）
                    Decimator = Decimator.Decimate,  // 启用数据缩减算法（提升渲染性能）
                    RenderInLegend = true           // 不在图例中显示
                };
                lineSeriesList.Add(twoColorSeries);
                plotModel.Series.Add(twoColorSeries);

                LineSeries twoColorSeries2 = new TwoColorLineSeries
                {
                    Color = config.TwoColorLineSeries.Color2,
                    StrokeThickness = config.lineThickness,             // 线条粗细
                    MarkerType = MarkerType.None,    // 禁用数据点标记（提升性能）
                    Decimator = Decimator.Decimate,  // 启用数据缩减算法（提升渲染性能）
                    RenderInLegend = true           // 不在图例中显示
                };
                lineSeriesList.Add(twoColorSeries2);
                plotModel.Series.Add(twoColorSeries2);
            }

            return plotModel;
        }

        /// <summary>
        /// 批量添加数据点（支持多曲线同步更新）
        /// </summary>
        /// <param name="data">可变参数数组，每个JArray对应一条曲线的数据</param>
        /// <example>
        /// addPointArray(
        ///     new JArray{1,2,3},  // 曲线1数据
        ///     new JArray{4,5,6}   // 曲线2数据
        /// )
        /// </example>
        public void addPointArray(params JArray[] data)
        {
            // 遍历每个数据点索引
            for (int i = 0; i < data[0].Count; i++)
            {
                // 遍历每条曲线
                for (int j = 0; j < data.Length; j++)
                {
                    // 添加数据点到对应曲线
                    lineSeriesList[j].Points.Add(new DataPoint(
                        timeCounter,          // X坐标为时间计数器
                        (double)data[j][i]    // Y坐标为数据值
                    ));

                    // 维护数据窗口长度（先进先出）
                    if (lineSeriesList[j].Points.Count > max_x)
                    {
                        lineSeriesList[j].Points.RemoveAt(0);
                    }
                }
                timeCounter++;  // 更新全局计数器
            }

            // 自动滚动X轴显示窗口
            if (timeCounter > max_x)
            {
                var xAxis = plotModel.Axes[0] as LinearAxis;
                xAxis.Minimum = timeCounter - max_x;  // 窗口起点
                xAxis.Maximum = timeCounter;          // 窗口终点
            }

            // 请求重绘图表（true表示异步更新）
            plotModel.InvalidatePlot(true);
        }

 
        public void addPointNum(params double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                lineSeriesList[i].Points.Add(new DataPoint(timeCounter, data[i]));
                // 保持固定时间窗口
                if (lineSeriesList[i].Points.Count > max_x)
                {
                    lineSeriesList[i].Points.RemoveAt(0);
                }
            }

            timeCounter++;

            // 自动滚动X轴
            if (timeCounter > max_x)
            {
                var xAxis = plotModel.Axes[0] as LinearAxis;
                xAxis.Minimum = timeCounter - max_x;
                xAxis.Maximum = timeCounter;
            }
            plotModel.InvalidatePlot(true);

        }
    }
}









/**
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace wpf_test_sdk.Tools
{
    internal class OxyChart
    {
        private PlotModel plotModel;

        private List<LineSeries> lineSeriesList = new List<LineSeries>();

        private int timeCounter = 0;

        OxyColor[] colors = { OxyColors.Green, OxyColors.GreenYellow, OxyColors.Honeydew, OxyColors.HotPink, OxyColors.Ivory };

        private float max_x;
        private float max_y;
        private int lineCount;


        public OxyChart(string title, float x_size, float y_size, int lineCount)
        {
            plotModel = new PlotModel();
            max_x = x_size;
            max_y = y_size;
            this.lineCount = lineCount;
        }


        public PlotModel init()
        {
            plotModel.Background = OxyColor.Parse("#110C2C");
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "采样点",
                Minimum = 0,
                Maximum = max_x
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "数值",
                Minimum = 0,
                Maximum = max_y
            });

            for (int i = 0; i < lineCount; i++)
            {
                LineSeries realTimeSeries = new LineSeries
                {
                    Color = colors[i],
                    StrokeThickness = 2,
                    // 禁用标记提高性能
                    MarkerType = MarkerType.None,
                    // 使用数据缩减算法
                    Decimator = Decimator.Decimate,
                    // 使用更快的渲染方法
                    RenderInLegend = false,

                };
                lineSeriesList.Add(realTimeSeries);
                plotModel.Series.Add(realTimeSeries);
            }

            return plotModel;


        }


        public void addPointArray(params JArray[] data)
        {
            for (int i = 0; i < data[0].Count; i++)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    lineSeriesList[j].Points.Add(new DataPoint(timeCounter, (double)data[j][i]));
                    // 保持固定时间窗口
                    if (lineSeriesList[j].Points.Count > max_x)
                    {
                        lineSeriesList[j].Points.RemoveAt(0);
                    }
                }
                timeCounter++;
            }


            // 自动滚动X轴
            if (timeCounter > max_x)
            {
                var xAxis = plotModel.Axes[0] as LinearAxis;
                xAxis.Minimum = timeCounter - max_x;
                xAxis.Maximum = timeCounter;
            }
            // 刷新图表
            plotModel.InvalidatePlot(true);


        }

        public void addPointNum(params double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                lineSeriesList[i].Points.Add(new DataPoint(timeCounter, data[i]));
                // 保持固定时间窗口
                if (lineSeriesList[i].Points.Count > max_x)
                {
                    lineSeriesList[i].Points.RemoveAt(0);
                }
            }

            timeCounter++;

            // 自动滚动X轴
            if (timeCounter > max_x)
            {
                var xAxis = plotModel.Axes[0] as LinearAxis;
                xAxis.Minimum = timeCounter - max_x;
                xAxis.Maximum = timeCounter;
            }
            plotModel.InvalidatePlot(true);

        }
    }
}
**/

