


using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace EEG.Tool
{
    public class OxyChart:ViewModelBase
    {
        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set
            {
                plotModel = value;
                RaisePropertyChanged(() => PlotModel);
            }
        }
        private List<LineSeries> lineSeriesList = new List<LineSeries>();
        private int timeCounter = 0;
        private PlotController _plotController;
        public PlotController plotController
        {
            get { return _plotController; }
            set
            {
                _plotController = value;
                RaisePropertyChanged(() => plotController);
            }
        }

        // 默认颜色池
        private OxyColor[] defaultColors = new OxyColor[]
        {
            OxyColors.Green, OxyColors.Red, OxyColors.Blue,
            OxyColors.Orange, OxyColors.Purple, OxyColors.Cyan
        };

        private double max_x;
        private double max_y;
        private int lineCount;
        private string[] legendTitles;
        private OxyColor[] lineColors;
        private LegendPosition legendPosition;

        /// <summary>
        /// 构造：兼容 C# 7.3（不要使用 nullable ? 或 target-typed new）
        /// legendTitles / lineColors 可为 null（会被替换为空数组）
        /// </summary>
        public OxyChart(
            string title,
            double xSize,
            double ySize,
            int lineCount,
            string[] legendTitles = null,
            OxyColor[] lineColors = null,
            LegendPosition legendPosition = LegendPosition.TopRight)
        {


            plotController = new PlotController();

            // 禁用鼠标滚轮缩放
            plotController.UnbindMouseWheel();




            PlotModel = new PlotModel
            {
                Title = title,


                TitleFont = "Segoe UI",
                TitleFontSize = 15,
                TitleColor = OxyColor.Parse("#5B9BD5"),
                Background = OxyColor.Parse("#DEEBF6")
            };
            max_x = xSize;
            max_y = ySize;
            this.lineCount = lineCount;
            this.legendTitles = legendTitles ?? new string[0];
            this.lineColors = lineColors ?? new OxyColor[0];
            this.legendPosition = legendPosition;
        }

        /// <summary>
        /// 初始化 PlotModel（与原来 init() 命名保持一致）
        /// </summary>
        public PlotModel init()
        {
            PlotModel.Background = OxyColor.Parse("#DEEBF6");

            // X 轴
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                //Title = "Samples",
                TitleFont = "Segoe UI",
                Minimum = 0,
                Maximum = max_x,
                MajorStep = Math.Max(1.0, max_x / 10.0),


                AxislineColor = OxyColor.Parse("#5B9BD5"),
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#5B9BD5"),           // 刻度文字颜色
                MajorGridlineStyle = LineStyle.Solid,  // 主网格线
                MajorGridlineColor = OxyColor.FromAColor(30, OxyColors.Black),
                //MinorGridlineStyle = LineStyle.Dot,
                //MinorGridlineColor = OxyColor.FromAColor(30, OxyColors.Black),
                TicklineColor = OxyColors.White


            });

            // Y 轴
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                //Title = "Signal",
                Minimum = 0,
                Maximum = max_y,
                MajorStep = Math.Max(1.0, max_y / 10.0),
                AxislineColor = OxyColor.Parse("#5B9BD5"),
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColor.Parse("#5B9BD5"),           // 刻度文字颜色
                MajorGridlineStyle = LineStyle.Solid,  // 主网格线
                MajorGridlineColor = OxyColor.FromAColor(30, OxyColors.Black),
                //MinorGridlineStyle = LineStyle.Dot,
                //MinorGridlineColor = OxyColor.FromAColor(30, OxyColors.Black),
                TicklineColor = OxyColors.White
            });




            // 图例（外置）
            PlotModel.Legends.Add(new Legend
            {
                LegendPosition = legendPosition,
                LegendPlacement = LegendPlacement.Inside,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorderThickness = 0,
                LegendTextColor = OxyColor.Parse("#5B9BD5"),
                LegendMargin=-5, // 减少边距
                //LegendPlacement = LegendPlacement.Inside, // 放在绘图区内
                //LegendPosition = LegendPosition.TopCenter,  // 相对于绘图区的左上角
                //LegendOrientation = LegendOrientation.Horizontal,
                //LegendMargin = -20,                          // 减少边距
                //LegendPadding = 0,                         // 内边距
                //LegendBorderThickness = 0,
                //LegendTextColor = OxyColor.Parse("#5B9BD5")
            });

            // 添加曲线系列
            for (int i = 0; i < lineCount; i++)
            {
                LineSeries series = new LineSeries
                {
                    Title = (i < legendTitles.Length) ? legendTitles[i] : ("曲线" + (i + 1)),
                    Color = (i < lineColors.Length) ? lineColors[i] : defaultColors[i % defaultColors.Length],
                    StrokeThickness = 2,
                    MarkerType = MarkerType.None,
                    Decimator = Decimator.Decimate,
                };

                lineSeriesList.Add(series);
                plotModel.Series.Add(series);
            }

            return plotModel;
        }

        /// <summary>
        /// 批量添加数据（JArray 版本），保持你原有逻辑：逐点插入，超过窗口删除最前面点
        /// </summary>
        public void addPointArray(params JArray[] data)
        {
            if (data == null || data.Length == 0) return;

            for (int i = 0; i < data[0].Count; i++)
            {
                for (int j = 0; j < data.Length && j < lineSeriesList.Count; j++)
                {
                    double y = (double)data[j][i];
                    lineSeriesList[j].Points.Add(new DataPoint(timeCounter, y));

                    if (lineSeriesList[j].Points.Count > max_x)
                    {
                        lineSeriesList[j].Points.RemoveAt(0);
                    }
                }
                timeCounter++;
            }

            UpdateXAxis();
            plotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 批量添加数据（double[] 版本）
        /// </summary>
        public void addPointNum(params double[] data)
        {
            if (data == null || data.Length == 0) return;

            for (int i = 0; i < data.Length && i < lineSeriesList.Count; i++)
            {
                lineSeriesList[i].Points.Add(new DataPoint(timeCounter, data[i]));

                if (lineSeriesList[i].Points.Count > max_x)
                {
                    lineSeriesList[i].Points.RemoveAt(0);
                }
            }

            timeCounter++;
            UpdateXAxis();
            PlotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 更新 X 轴窗口（滚动）
        /// </summary>
        private void UpdateXAxis()
        {
            if (timeCounter > max_x)
            {
                var xAxis = PlotModel.Axes[0] as LinearAxis;
                if (xAxis != null)
                {
                    xAxis.Minimum = timeCounter - max_x;
                    xAxis.Maximum = timeCounter;
                }
            }
        }
    }
}
