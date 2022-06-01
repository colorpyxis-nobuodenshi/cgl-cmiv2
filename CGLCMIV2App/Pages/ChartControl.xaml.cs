using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CGLCMIV2App
{
    /// <summary>
    /// ChartControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ChartControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ChartControl), new PropertyMetadata(null));
        public static readonly DependencyProperty XLabelProperty = DependencyProperty.Register("XLabel", typeof(string), typeof(ChartControl), new PropertyMetadata(null));
        public static readonly DependencyProperty YLabelProperty = DependencyProperty.Register("YLabel", typeof(string), typeof(ChartControl), new PropertyMetadata(null));
        public static readonly DependencyProperty XAxisMinProperty = DependencyProperty.Register("XAxisMin", typeof(double), typeof(ChartControl), new PropertyMetadata(0.0));
        public static readonly DependencyProperty XAxisMaxProperty = DependencyProperty.Register("XAxisMax", typeof(double), typeof(ChartControl), new PropertyMetadata(100.0));
        public static readonly DependencyProperty YAxisMinProperty = DependencyProperty.Register("YAxisMin", typeof(double), typeof(ChartControl), new PropertyMetadata(0.0));
        public static readonly DependencyProperty YAxisMaxProperty = DependencyProperty.Register("YAxisMax", typeof(double), typeof(ChartControl), new PropertyMetadata(100.0));
        public static readonly DependencyProperty IsGridLineProperty = DependencyProperty.Register("IsGridLine", typeof(bool), typeof(ChartControl), new PropertyMetadata(true));
        public static readonly DependencyProperty IsAxisLabelProperty = DependencyProperty.Register("IsAxisLabel", typeof(bool), typeof(ChartControl), new PropertyMetadata(true));
        public static readonly DependencyProperty XAxisTickProperty = DependencyProperty.Register("XAxisTick", typeof(double), typeof(ChartControl), new PropertyMetadata(10.0));
        public static readonly DependencyProperty YAxisTickProperty = DependencyProperty.Register("YAxisTick", typeof(double), typeof(ChartControl), new PropertyMetadata(10.0));

        protected const int LEFT_MARGIN = 15;
        protected const int RIGHT_MARGIN = 15;
        protected const int TOP_MARGIN = 15;
        protected const int BOTTOM_MARGIN = 25;
        protected const int X_NUMBERS = 0x1;
        protected const int Y_NUMBERS = 0x2;
        protected const int X_TICKS = 0x4;
        protected const int Y_TICKS = 0x8;
        protected const int X_GRID = 0x10;
        protected const int Y_GRID = 0x20;
        protected const int DEFAULT_FLAGS = X_NUMBERS + Y_NUMBERS + X_GRID + Y_GRID;
        protected const int MAX_INTERVALS = 10;
        protected const int MIN_X_GRIDWIDTH = 10;
        protected const int MIN_Y_GRIDWIDTH = 10;
        protected const int TICK_LENGTH = 6;

        //protected double xMin;
        //protected double xMax;
        //protected double yMin;
        //protected double yMax;
        //protected bool fixedYScale = false;
        //protected int frameX, frameY, frameWidth, frameHeight;
        //protected int plotWidth, plotHeight;
        //protected double xScale, yScale;
        //protected int flags = DEFAULT_FLAGS;
        //protected System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily("Arial");
        //protected int fontSize = 12;
        private double leftOffset = LEFT_MARGIN;
        private double rightOffset = RIGHT_MARGIN;
        private double topffset = TOP_MARGIN;
        private double bottomOffset = BOTTOM_MARGIN;
        protected DispatcherTimer _timer;
        private Line gridline;

        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public string XLabel
        {
            get
            {
                return (string)GetValue(XLabelProperty);
            }
            set
            {
                SetValue(XLabelProperty, value);
            }
        }

        public string YLabel
        {
            get
            {
                return (string)GetValue(YLabelProperty);
            }
            set
            {
                SetValue(YLabelProperty, value);
            }
        }

        public double XAxisMin
        {
            get
            {
                return (double)GetValue(XAxisMinProperty);
            }
            set
            {
                SetValue(XAxisMinProperty, value);
            }
        }

        public double XAxisMax
        {
            get
            {
                return (double)GetValue(XAxisMaxProperty);
            }
            set
            {
                SetValue(XAxisMaxProperty, value);
            }
        }

        public double XAxisTick
        {
            get
            {
                return (double)GetValue(XAxisTickProperty);
            }
            set
            {
                SetValue(XAxisTickProperty, value);
            }
        }

        public double YAxisMin
        {
            get
            {
                return (double)GetValue(YAxisMinProperty);
            }
            set
            {
                SetValue(YAxisMinProperty, value);
            }
        }

        public double YAxisMax
        {
            get
            {
                return (double)GetValue(YAxisMaxProperty);
            }
            set
            {
                SetValue(YAxisMaxProperty, value);
            }
        }

        public double YAxisTick
        {
            get
            {
                return (double)GetValue(YAxisTickProperty);
            }
            set
            {
                SetValue(YAxisTickProperty, value);
            }
        }

        public bool IsGrid
        {
            get
            {
                return (bool)GetValue(IsGridLineProperty);
            }
            set
            {
                SetValue(IsGridLineProperty, value);
            }
        }

        public bool IsAxisLabel
        {
            get
            {
                return (bool)GetValue(IsAxisLabelProperty);
            }
            set
            {
                SetValue(IsAxisLabelProperty, value);
            }
        }

        public ChartControl()
        {
            InitializeComponent();

            ResizeChart();
        }


        public void DrawChartStyle()
        {
            var pt = new Point();
            //var tick = new Line();
            var offset = 0.0;
            var dx = 0.0;
            var dy = 0.0;
            var tb = new TextBlock();
            var xmin = XAxisMin;
            var xmax = XAxisMax;
            var ymin = YAxisMin;
            var ymax = YAxisMax;
            var xtick = XAxisTick;
            var ytick = YAxisTick;

            tb.Text = XAxisMax.ToString();
            tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var size = tb.DesiredSize;
            //rightOffset = 10;

            // Determine left offset:
            for (dy = ymin; dy <= ymax; dy += ytick)
            {
                pt = NormalizePoint(new Point(xmin, dy));
                tb = new TextBlock();
                tb.Text = dy.ToString();
                tb.TextAlignment = TextAlignment.Right;
                tb.Measure(new Size(Double.PositiveInfinity,
                                    Double.PositiveInfinity));
                size = tb.DesiredSize;
                if (offset < size.Width)
                    offset = size.Width;
            }
            leftOffset = offset + 10;
            Canvas.SetLeft(chartCanvas, leftOffset);
            Canvas.SetBottom(chartCanvas, bottomOffset);
            chartCanvas.Width = Math.Abs(textCanvas.Width - leftOffset - rightOffset);
            chartCanvas.Height = Math.Abs(textCanvas.Height - bottomOffset - size.Height / 2);

            var chartRect = new Rectangle();
            chartRect.Stroke = Brushes.DarkGray;
            chartRect.Width = chartCanvas.Width;
            chartRect.Height = chartCanvas.Height;
            chartCanvas.Children.Add(chartRect);



            // Create vertical gridlines:
            if (IsGrid == true)
            {
                for (dx = xmin + xtick; dx < xmax; dx += xtick)
                {
                    gridline = new Line();
                    gridline.StrokeThickness = 1;
                    gridline.Stroke = Brushes.DarkGray;
                    gridline.StrokeDashArray = new DoubleCollection { 3, 2 };
                    gridline.StrokeDashCap = PenLineCap.Round;
                    gridline.SnapsToDevicePixels = true;
                    gridline.X1 = NormalizePoint(new Point(dx, ymin)).X;
                    gridline.Y1 = NormalizePoint(new Point(dx, ymin)).Y;
                    gridline.X2 = NormalizePoint(new Point(dx, ymax)).X;
                    gridline.Y2 = NormalizePoint(new Point(dx, ymax)).Y;
                    chartCanvas.Children.Add(gridline);
                }
            }

            // Create horizontal gridlines:
            if (IsGrid == true)
            {
                for (dy = ymin + ytick; dy < ymax; dy += ytick)
                {
                    gridline = new Line();
                    gridline.StrokeThickness = 1;
                    gridline.Stroke = Brushes.DarkGray;
                    gridline.StrokeDashArray = new DoubleCollection { 3, 2 };
                    gridline.StrokeDashCap = PenLineCap.Round;
                    gridline.SnapsToDevicePixels = true;
                    gridline.X1 = NormalizePoint(new Point(xmin, dy)).X;
                    gridline.Y1 = NormalizePoint(new Point(xmin, dy)).Y;
                    gridline.X2 = NormalizePoint(new Point(xmax, dy)).X;
                    gridline.Y2 = NormalizePoint(new Point(xmax, dy)).Y;
                    chartCanvas.Children.Add(gridline);
                }
            }

            // Create x-axis tick marks:
            for (dx = xmin; dx <= xmax; dx += xtick)
            {
                pt = NormalizePoint(new Point(dx, ymin));
                //tick = new Line();
                //tick.Stroke = Brushes.Black;
                //tick.X1 = pt.X;
                //tick.Y1 = pt.Y;
                //tick.X2 = pt.X;
                //tick.Y2 = pt.Y - 5;
                //chartCanvas.Children.Add(tick);

                tb = new TextBlock();
                tb.Text = string.Format("{0:0.##}", dx);//string.Format(CultureInfo.InvariantCulture, "{0:0.00#E+0}", dx);
                tb.Measure(new Size(Double.PositiveInfinity,
                                    Double.PositiveInfinity));
                size = tb.DesiredSize;
                textCanvas.Children.Add(tb);
                Canvas.SetLeft(tb, leftOffset + pt.X - size.Width / 2);
                Canvas.SetTop(tb, pt.Y + 2 + size.Height / 2);

            }

            // Create y-axis tick marks:
            for (dy = ymin; dy <= ymax; dy += ytick)
            {
                pt = NormalizePoint(new Point(xmin, dy));
                //tick = new Line();
                //tick.Stroke = Brushes.Black;
                //tick.X1 = pt.X;
                //tick.Y1 = pt.Y;
                //tick.X2 = pt.X + 5;
                //tick.Y2 = pt.Y;
                //chartCanvas.Children.Add(tick);

                tb = new TextBlock();
                if (dy == 0)
                {
                    tb.Text = string.Format("{0:0.###}", dy);
                }
                else
                {
                    tb.Text = string.Format("{0:0.###}", dy);// string.Format(CultureInfo.InvariantCulture, "{0:0.00#E+0}", dy);
                }
                tb.Measure(new Size(Double.PositiveInfinity,
                                    Double.PositiveInfinity));
                size = tb.DesiredSize;
                textCanvas.Children.Add(tb);
                Canvas.SetRight(tb, chartCanvas.Width + rightOffset);
                Canvas.SetTop(tb, pt.Y);
            }

            // Add title and labels:
            //tbTitle.Text = Title;
            //tbXLabel.Text = XLabel;
            //tbYLabel.Text = YLabel;
            //tbXLabel.Margin = new Thickness(leftOffset + 2, 2, 2, 2);
            //tbTitle.Margin = new Thickness(leftOffset + 2, 2, 2, 2);
        }


        public Point NormalizePoint(Point pt)
        {
            if (Double.IsNaN(chartCanvas.Width) || chartCanvas.Width <= 0)
            {
                chartCanvas.Width = 300;
            }
            if (Double.IsNaN(chartCanvas.Height) || chartCanvas.Height <= 0)
            {
                chartCanvas.Height = 300;
            }

            var result = new Point();
            result.X = (pt.X - XAxisMin) * chartCanvas.Width / (XAxisMax - XAxisMin);
            result.Y = chartCanvas.Height - (pt.Y - YAxisMin) * chartCanvas.Height / (YAxisMax - YAxisMin);

            

            return result;
        }

        public Point Screen2Point(Point pt)
        {
            if (Double.IsNaN(chartCanvas.Width) || chartCanvas.Width <= 0)
            {
                chartCanvas.Width = 300;
            }
            if (Double.IsNaN(chartCanvas.Height) || chartCanvas.Height <= 0)
            {
                chartCanvas.Height = 300;
            }

            var result = new Point();
            result.X = (((pt.X)  * (XAxisMax - XAxisMin) / (chartCanvas.Width)) + XAxisMin);
            result.Y = ((chartCanvas.Height - (pt.Y)) * (YAxisMax - YAxisMin) / chartCanvas.Height) + YAxisMin;

            result.X = Math.Truncate(result.X * 1000.0) / 1000.0;
            result.Y = Math.Truncate(result.Y * 1000.0) / 1000.0;

            return result;
        }

        public void Clear()
        {
            chartCanvas.Children.Clear();
            textCanvas.Children.RemoveRange(1, textCanvas.Children.Count-1);
        }

        public virtual void Draw()
        {
            Clear();
            DrawChartStyle();
        }

        public virtual void ResizeChart()
        {
            Clear();
            DrawChartStyle();
        }

        private void chartGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeChart();
        }


    }


    

    
}
