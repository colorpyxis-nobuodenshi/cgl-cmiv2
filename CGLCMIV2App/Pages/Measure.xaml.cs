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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using CGLCMIV2.Application;
using CGLCMIV2.Domain;
using System.Threading;
using Linearstar.Windows.RawInput;
using System.Windows.Interop;

namespace CGLCMIV2App
{
    /// <summary>
    /// Measure.xaml の相互作用ロジック
    /// </summary>
    public partial class Measure : UserControl
    {
        public Measure(IServiceProvider serviceProvider)
        {
            InitializeComponent();


            var barcode = string.Empty;
            var measureCancellationTokenSource = new CancellationTokenSource();
            //var condition = serviceProvider.GetService<AppStore>().ColorimetryCondition;
            var autoMeasure = serviceProvider.GetService<AutoColorMeasuringStart>();
            var measure = serviceProvider.GetService<ColorMeasuringStart>();
            var move1 = serviceProvider.GetService<AutoStageMoveMeasurePoint>();
            var move2 = serviceProvider.GetService<AutoStageMoveWorkSetPoint>();
            var rotate1 = serviceProvider.GetService<AutoStageRotateCW45>();
            var rotate2 = serviceProvider.GetService<AutoStageRotateCCW45>();
            var rotate3 = serviceProvider.GetService<AutoStageRotateCWJ>();
            var rotate4 = serviceProvider.GetService<AutoStageRotateCCWJ>();
            var cameraCaptureStart = serviceProvider.GetService<CameraCaptureStart>();
            var cameraCaptureStop = serviceProvider.GetService<CameraCaptureStop>();
            var cameraCaptureCancellationTokenSource = new CancellationTokenSource();
            var appStore = serviceProvider.GetService<AppStore>();
            //var condition = appStore.ColorimetryCondition;
            HwndSource source = null;
            message.Text = string.Empty;

            mode1.Checked += delegate
            {
                if (mode1.IsChecked == true)
                {
                    //btnMeasure.IsEnabled = false;
                    btnMove1.IsEnabled = false;
                    btnMove2.IsEnabled = false;
                    btnRotate1.IsEnabled = false;
                    btnRotate2.IsEnabled = false;
                    btnRotate3.IsEnabled = false;
                    btnRotate4.IsEnabled = false;
                }
                if (mode2.IsChecked == true)
                {
                    //btnMeasure.IsEnabled = true;
                    btnMove1.IsEnabled = true;
                    btnMove2.IsEnabled = true;
                    btnRotate1.IsEnabled = true;
                    btnRotate2.IsEnabled = true;
                    btnRotate3.IsEnabled = true;
                    btnRotate4.IsEnabled = true;
                }
            };
            mode2.Checked += delegate
            {
                if (mode1.IsChecked == true)
                {
                    //btnMeasure.IsEnabled = false;
                    btnMove1.IsEnabled = false;
                    btnMove2.IsEnabled = false;
                    btnRotate1.IsEnabled = false;
                    btnRotate2.IsEnabled = false;
                    btnRotate3.IsEnabled = false;
                    btnRotate4.IsEnabled = false;
                }
                if (mode2.IsChecked == true)
                {
                    //btnMeasure.IsEnabled = true;
                    btnMove1.IsEnabled = true;
                    btnMove2.IsEnabled = true;
                    btnRotate1.IsEnabled = true;
                    btnRotate2.IsEnabled = true;
                    btnRotate3.IsEnabled = true;
                    btnRotate4.IsEnabled = true;
                }
            };

            btnMeasure.Click += async delegate
            {
                var condition = appStore.ColorimetryCondition;
                measureCancellationTokenSource = new CancellationTokenSource();
                if (mode1.IsChecked == true)
                {
                    btnMeasure.IsEnabled = false;
                    message.Text = "測定中しばらくお待ちください";
                    await autoMeasure.ExecuteAsync(serialNumber.Text.Trim(), condition, measureCancellationTokenSource);
                    btnMeasure.IsEnabled = true;
                    return;
                }
                btnMeasure.IsEnabled = false;
                await measure.ExecuteAsync(serialNumber.Text, condition);
                btnMeasure.IsEnabled = true;
            };
            btnMove1.Click += async delegate
            {
                btnMove1.IsEnabled = false;
                await move1.ExecuteAsync();
                btnMove1.IsEnabled = true;
            };
            btnMove2.Click += async delegate
            {
                btnMove2.IsEnabled = false;
                await move2.ExecuteAsync();
                btnMove2.IsEnabled = true;
            };
            btnRotate1.Click += async delegate
            {
                btnRotate1.IsEnabled = false;
                await rotate1.ExecuteAsync();
                btnRotate1.IsEnabled = true;
            };
            btnRotate2.Click += async delegate
            {
                btnRotate2.IsEnabled = false;
                await rotate2.ExecuteAsync();
                btnRotate2.IsEnabled = true;
            };
            btnRotate3.Click += async delegate
            {
                btnRotate3.IsEnabled = false;
                await rotate3.ExecuteAsync();
                btnRotate3.IsEnabled = true;
            };
            btnRotate4.Click += async delegate
            {
                btnRotate4.IsEnabled = false;
                await rotate4.ExecuteAsync();
                btnRotate4.IsEnabled = true;
            };
            Loaded += delegate
            {
                var condition = appStore.ColorimetryCondition;
                cameraCaptureCancellationTokenSource = new CancellationTokenSource();
                cameraCaptureStart.Execute((o) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        var p = DisplayPixels.Convert(o);
                        var w = p.Width;
                        var h = p.Height;
                        var stride = w * 4;
                        var bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                        var len = stride * h;
                        bmp.WritePixels(new Int32Rect(0, 0, w, h), p.Pix, stride, 0, 0);
                        monitor.Source = bmp;
                    });
                }, cameraCaptureCancellationTokenSource, condition.ExposureTime, 1, condition.ShadingCorrectPixels);

                RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.NoLegacy, new WindowInteropHelper(ViewController.Instance.MainWindow).Handle);


                source = HwndSource.FromHwnd(new WindowInteropHelper(ViewController.Instance.MainWindow).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            };

            Unloaded += delegate
            {
                cameraCaptureStop.Execute(cameraCaptureCancellationTokenSource);

                RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);

                source.RemoveHook(new HwndSourceHook(WndProc));
            };


            appStore.Subscribe<AutoColorMeasuringCompletedEvent>(r =>
            {
                var res = r.Report;
                var X = res.XYZ.X;
                var Y = res.XYZ.Y;
                var Z = res.XYZ.Z;
                var l = res.LAB.L;
                var a = res.LAB.A;
                var b = res.LAB.B;
                var c = res.LCH.C;
                var h = res.LCH.H;
                var x = (X + Y + Z) == 0 ? 0 : X / (X + Y + Z);
                var y = (X + Y + Z) == 0 ? 0 : Y / (X + Y + Z);
                ResultLAB.Text = string.Format("{0:F1}, {1:F3}, {2:F3}", l, a, b);
                ResultLCH.Text = string.Format("{0:F1}, {1:F3}, {2:F1}", l, c, h);
                ResultXYZ.Text = string.Format("{0:F0}, {1:F0}, {2:F0}", X, Y, Z);

                ResultLabel1.Text = "L*";
                ResultLabel2.Text = "C*";
                ResultLabel3.Text = "h*";
                Result1.Text = string.Format("{0:F1}", l);
                Result2.Text = string.Format("{0:F3}", c);
                Result3.Text = string.Format("{0:F1}", h);

                var lab = string.Format("{0:F1}, {1:F3}, {2:F3}", l, a, b);
                var lch = string.Format("{0:F1}, {1:F3}, {2:F1}", l, c, h);
                var xyz = string.Format("{0:F0}, {1:F0}, {2:F0}", X, Y, Z);
                listView1.Items.Insert(0, new string[] { serialNumber.Text, lch, xyz, lab });

                (double rangeMinA, double rangeMaxA, double rangeMinB, double rangeMaxB) CalclateRangeReasultLabRange(double l, double a, double b)
                {
                    (double min, double max) f(double val)
                    {
                        if (Math.Abs(val) <= 1.0)
                        {
                            return (-1.0, 1.0);
                        }
                        else if (Math.Abs(val) > 1.0 && Math.Abs(val) <= 5.0)
                        {
                            return (-5.0, 5.0);
                        }
                        else if (Math.Abs(val) > 5.0 && Math.Abs(val) <= 10.0)
                        {
                            return (-10.0, 10.0);
                        }
                        else if (Math.Abs(val) > 10.0 && Math.Abs(val) <= 20.0)
                        {
                            return (-20.0, 20.0);
                        }
                        else if (Math.Abs(val) > 20.0 && Math.Abs(val) <= 50.0)
                        {
                            return (-50.0, 50.0);
                        }

                        return (-50.0, 50.0);
                    }

                    var c = Math.Max(Math.Abs(a), Math.Abs(b));
                    var range = f(c);

                    return (range.min, range.max, range.min, range.max);
                }

                var labRange = CalclateRangeReasultLabRange(l, a, b);
                labChart1.XAxisMin = labRange.rangeMinA;
                labChart1.XAxisMax = labRange.rangeMaxA;
                labChart1.YAxisMin = labRange.rangeMinB;
                labChart1.YAxisMax = labRange.rangeMaxB;
                labChart1.XAxisTick = labRange.rangeMaxA / 2;
                labChart1.YAxisTick = labRange.rangeMaxB / 2;
                labChart1.Lab = new CIELABChartValue() { a = a, b = b };

                message.Text = string.Empty;

            });

            appStore.Subscribe<AutoColorMeasuringColorimetryResultChangedEvent>(r =>
            {
                var o = r.Result.Pixels;
                var ma = r.Result.MeasureArea;
                var p = DisplayPixels.Convert(o, ma);
                var w = p.Width;
                var h = p.Height;
                var stride = w * 4;
                var bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                var len = stride * h;
                bmp.WritePixels(new Int32Rect(0, 0, w, h), p.Pix, stride, 0, 0);
                capture.Source = bmp;
            });

            appStore.Subscribe<AppAbortedEvent>(_ =>
            {
                measureCancellationTokenSource?.Cancel();
                message.Text = "";
            });

            IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                const int WM_INPUT = 0x00FF;
                const int WM_KEYDOWN = 0x100;
                const int WM_KEYUP = 0x101;

                const int VK_F1 = 0x70;
                const int VK_F2 = 0x71;
                const int VK_F3 = 0x72;
                const int VK_F4 = 0x73;
                const int VK_F5 = 0x74;
                const int VK_F6 = 0x75;
                const int VK_F7 = 0x76;
                const int VK_F8 = 0x77;
                const int VK_F9 = 0x78;
                const int VK_F10 = 0x79;
                const int VK_F11 = 0x7A;
                const int VK_F12 = 0x7B;
                const int VK_TAB = 0x09;
                const int VK_SHIFT = 0x10;

                if (msg == WM_INPUT)
                {
                    var data = RawInputData.FromHandle(lParam);
                    switch (data)
                    {
                        case RawInputMouseData mouse:
                            break;
                        case RawInputKeyboardData keyboard:
                            if (keyboard.Keyboard.WindowMessage == WM_KEYDOWN)
                            {
                                //if (keyboard.Device.VendorId == 1211 && keyboard.Device.ProductId == 3868)
                                {
                                    var key = keyboard.Keyboard.VirutalKey;

                                    if (key == VK_TAB)//TAB KEY
                                    {
                                        barcode = barcode.Trim();
                                        if (barcode.Length != 8)
                                        {
                                            MessageBox.Show("バーコードの読み取りに失敗しました.");
                                            serialNumber.Text = string.Empty;
                                        }
                                        else
                                        {
                                            serialNumber.Text = barcode;
                                        }
                                        barcode = string.Empty;
                                    }

                                    if (key != VK_SHIFT)//SHIFT KEY
                                    {
                                        if ((key >= 0x30 && key <= 0x39) || (key >= 0x41 && key <= 0x5A))
                                        {
                                            barcode += ((char)keyboard.Keyboard.VirutalKey).ToString();
                                        }
                                    }
                                }
                            }
                            break;
                        case RawInputHidData hid:
                            break;
                    }
                }

                return IntPtr.Zero;
            }
        }

    }
}
