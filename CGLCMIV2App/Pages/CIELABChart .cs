using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CGLCMIV2App
{
    public class CIELABChartValue : DependencyObject
    {
        public static readonly DependencyProperty aProperty = DependencyProperty.Register("a", typeof(double), typeof(CIELABChartValue), new PropertyMetadata(null));
        public static readonly DependencyProperty bProperty = DependencyProperty.Register("b", typeof(double), typeof(CIELABChartValue), new PropertyMetadata(null));

        public CIELABChartValue()
        {

        }

        public double a
        {
            get
            {
                return (double)GetValue(aProperty);
            }
            set
            {
                SetValue(aProperty, value);
            }
        }

        public double b
        {
            get
            {
                return (double)GetValue(bProperty);
            }
            set
            {
                SetValue(bProperty, value);
            }
        }

        public Color FillColor
        {
            get;
            set;
        }

        public Color StrokColor
        {
            get;set;
        }
    }

    public class CIELABChart : ChartControl
    {


        public static readonly DependencyProperty CIELABProperty = DependencyProperty.Register("Lab", typeof(CIELABChartValue), typeof(CIELABChart), new PropertyMetadata(null, (_, __) => { ((CIELABChart)_).Draw(); }));
        public static readonly DependencyProperty CIELABReferenceProperty = DependencyProperty.Register("LabReference", typeof(CIELABChartValue), typeof(CIELABChart), new PropertyMetadata(null, (_, __) => { ((CIELABChart)_).Draw(); }));
        public static readonly DependencyProperty DoSetPointProperty = DependencyProperty.Register("DoSetPoint", typeof(bool), typeof(CIELABChart), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (_, __) => { }));

        public CIELABChart() : base()
        {
            XAxisMin = -128.0;
            XAxisMax = 128.0;
            XAxisTick = 32;
            XLabel = "a*";
            YAxisMin = -128.0;
            YAxisMax = 128.0;
            YAxisTick = 32;
            YLabel = "b*";

            MouseDown += (s, e) =>
            {
                if (DoSetPoint)
                {
                    var pt = Screen2Point(e.GetPosition(chartCanvas));
                    Lab = new CIELABChartValue() { a = pt.X, b = pt.Y };

                    Draw();
                }
            };
        }

        public bool DoSetPoint
        {
            get
            {
                return (bool)GetValue(DoSetPointProperty);
            }
            set
            {
                SetValue(DoSetPointProperty, value);
            }
        }

        public CIELABChartValue Lab
        {
            get
            {
                return (CIELABChartValue)GetValue(CIELABProperty);
            }
            set
            {
                SetValue(CIELABProperty, value);
            }
        }

        public CIELABChartValue LabReference
        {
            get
            {
                return (CIELABChartValue)GetValue(CIELABReferenceProperty);
            }
            set
            {
                SetValue(CIELABReferenceProperty, value);
            }
        }

        public override void Draw()
        {
            base.Draw();

            DrawDiagramArea();

            DrawPlot(Lab, Colors.DarkBlue);

            DrawPlot(LabReference, Colors.LimeGreen);

            

        }

        public override void ResizeChart()
        {
            base.ResizeChart();

            DrawDiagramArea();

            DrawPlot(Lab, Colors.SteelBlue);

            DrawPlot(LabReference, Colors.LimeGreen);
        }

        public void DrawData()
        {

            
        }

        public virtual void DrawDiagramArea()
        {
            var wbWidth = 100;
            var wbHeight = 100;
            var bitmapImage = new BitmapImage();
            var bmp = new WriteableBitmap(wbWidth, wbHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            bmp.Lock();
            unsafe
            {
                var pBackBuffer = (int*)bmp.BackBuffer;

                //var ya = 0.1;
                //var sigmax = 3.0;
                //var sigmay = 3.0;
                //var xw = 0.31;
                //var yw = 0.31;

                for (var b = 100; b >= -100; b -= 2)
                {
                    for (var a = -100; a < 100; a += 2)
                    {
                        var rgb = XYZ2RGB(Lab2XYZ(new double[] { 60, a, b }));

                        var color = ((100 & 0xFF) << 24) | ((rgb[0] & 0xFF) << 16) | ((rgb[1] & 0xFF) << 8) | (rgb[2] & 0xFF);
                        *(pBackBuffer) = color;

                        pBackBuffer++;
                    }
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, wbWidth, wbHeight));
            bmp.Unlock();


            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }


            var poly = new Rectangle() { Width = chartCanvas.Width, Height = chartCanvas.Height, Fill = new ImageBrush() { ImageSource = bitmapImage } };

            chartCanvas.Children.Add(poly);
            
        }

        public virtual void DrawPlot(CIELABChartValue lab, Color color)
        {
            if (lab == null)
                return;

            var pt = NormalizePoint(new Point(lab.a, lab.b));
            var el = new Rectangle();
            el.Fill = new SolidColorBrush(color);
            //el.Stroke = Brushes.White;
            el.Height = 3;
            el.Width = 3;
            Canvas.SetTop(el, pt.Y - 1);
            Canvas.SetLeft(el, pt.X - 1);
            chartCanvas.Children.Add(el);
        }

        private double[] Lab2XYZ(double[] lab)
        {
            var l = lab[0];
            var a = lab[1];
            var b = lab[2];

            var fy = (l + 16) / 116.0;
            var fx = fy + (a / 500.0);
            var fz = fy - (b / 200.0);

            var Xn = 0.95;
            var Yn = 1.0;
            var Zn = 1.089;

            var Y = (fy > 6.0 / 29.0) ? (Math.Pow(fy, 3.0) * Yn) : Math.Pow(3.0 / 29.0, 3.0) * (116.0 * fy - 16.0) * Yn;
            var X = (fx > 6.0 / 29.0) ? (Math.Pow(fx, 3.0) * Xn) : Math.Pow(3.0 / 29.0, 3.0) * (116.0 * fx - 16.0) * Xn;
            var Z = (fz > 6.0 / 29.0) ? (Math.Pow(fz, 3.0) * Zn) : Math.Pow(3.0 / 29.0, 3.0) * (116.0 * fz - 16.0) * Zn;

            return new double[] { X, Y, Z};
        }

        private int[] XYZ2RGB(double[] XYZ)
        {
            var rgb = new int[3];
            var Y = XYZ[1];
            var X = XYZ[0];
            var Z = XYZ[2];


            var r = (3.2406 * X - 1.5372 * Y - 0.4986 * Z);
            var g = (-0.9689 * X + 1.8758 * Y + 0.0415 * Z);
            var b = (0.0557 * X - 0.2040 * Y + 1.0570 * Z);

            //var r = (1.326716 * X - 0.19248 * Y - 0.35264 * Z);
            //var g = (-0.42998 * X + 1.180085 * Y + 0.105711 * Z);
            //var b = (0.026461 * X - 0.07262 * Y + 1.408747 * Z);

            if (r <= 0.0031308)
            {
                r = r * 12.92;
            }
            else
            {
                r = Math.Pow(r, 1.0 / 2.4) * 1.055 - 0.055;
            }

            if (g <= 0.0031308)
            {
                g = g * 12.92;
            }
            else
            {
                g = Math.Pow(g, 1.0 / 2.4) * 1.055 - 0.055;
            }

            if (b <= 0.0031308)
            {
                b = b * 12.92;
            }
            else
            {
                b = Math.Pow(b, 1.0 / 2.4) * 1.055 - 0.055;
            }

            if (r < 0.0)
            {
                r = 0.0;
            }
            else if (r > 1.0)
            {
                r = 1.0;
            }
            else
            {
                r = r * r * (3 - 2 * r);
            }


            if (g < 0.0)
            {
                g = 0.0;
            }
            else if (g > 1.0)
            {
                g = 1.0;
            }
            else
            {
                g = g * g * (3 - 2 * g);
            }

            if (b < 0.0)
            {
                b = 0.0;
            }
            else if (b > 1.0)
            {
                b = 1.0;
            }
            else
            {
                b = b * b * (3 - 2 * b);
            }

            r = (int)(r * 255);
            g = (int)(g * 255);
            b = (int)(b * 255);


            rgb[0] = r > 255 ? 255 : r < 0 ? 0 : (int)r;
            rgb[1] = g > 255 ? 255 : g < 0 ? 0 : (int)g;
            rgb[2] = b > 255 ? 255 : b < 0 ? 0 : (int)b;

            return rgb;
        }
    }
}
