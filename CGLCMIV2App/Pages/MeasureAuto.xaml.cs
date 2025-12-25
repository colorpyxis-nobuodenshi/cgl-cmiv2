using System;
using System.Collections.Generic;
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
using System.Threading;
using CGLCMIV2.Domain;
using Linearstar.Windows.RawInput;
using System.Windows.Interop;

namespace CGLCMIV2App
{
    /// <summary>
    /// MeasureAuto.xaml の相互作用ロジック
    /// </summary>
    public partial class MeasureAuto : UserControl
    {

        public MeasureAuto(IServiceProvider serviceProvider)
        {
            InitializeComponent();


            var currentReports = new GradingReports(new TextBlock[] { txtNo1, txtNo2, txtNo3, txtNo4, txtNo5, txtNo6, txtNo7, txtNo8, txtNo9, txtNo10 }
                   , new TextBlock[] { txtGrade1, txtGrade2, txtGrade3, txtGrade4, txtGrade5, txtGrade6, txtGrade7, txtGrade8, txtGrade9, txtGrade10 }
                   , new TextBlock[] { txth1, txth2, txth3, txth4, txth5, txth6, txth7, txth8, txth9, txth10 });

            var prevReports = new GradingReports(new TextBlock[] { txtNo11, txtNo12, txtNo13, txtNo14, txtNo15, txtNo16, txtNo17, txtNo18, txtNo19, txtNo20 }
                , new TextBlock[] { txtGrade11, txtGrade12, txtGrade13, txtGrade14, txtGrade15, txtGrade16, txtGrade17, txtGrade18, txtGrade19, txtGrade20 }
                , new TextBlock[] { txth11, txth12, txth13, txth14, txth15, txth16, txth17, txth18, txth19, txth20 });
            var reportIndex = 0;

            var autoGradingCancellationTokenSource = new CancellationTokenSource();
            var appStore = serviceProvider.GetService<AppStore>();
            //var condition = appStore.ColorimetryCondition;
            //var condition2 = appStore.ColorGradingCondition;
            var barcode = string.Empty;
            var autogradingStarted = false;
            var autoGrading = serviceProvider.GetService<AutoGradingStart>();
            var barcodeRead = serviceProvider.GetService<BarcodeRead>();

            messagePanel1.Visibility = Visibility.Visible;
            messagePanel2.Visibility = Visibility.Hidden;

            currentReports.RemoveAll();
            prevReports.RemoveAll();

            appStore.Subscribe<AutoColorGradingCompletedEvent>(r =>
            {

                var cr = r.Report.ColorimetryReport;
                var cg = r.Report.ColorGrade;
                var lch = cr.LCH;
                var grade = cg.ToString();

                colorGrade.Text = grade;
                lchH.Text = string.Format("{0:F2}", lch.H);
                lchH.Background = grade.EndsWith("↑") ? Brushes.DarkBlue : grade.EndsWith("↓") ? Brushes.DarkRed : Brushes.Transparent;

                if (reportIndex >= 10)
                {
                    reportIndex = 0;
                    currentReports.Copy(prevReports);
                    currentReports.RemoveAll();
                }

                currentReports.Change(reportIndex, serialNumber.Text, grade, lchH.Text);
                reportIndex++;

                currentReport.Visibility = Visibility.Visible;
                prevReport.Visibility = Visibility.Collapsed;

                //message.Text = string.Empty;
                messagePanel1.Visibility = Visibility.Visible;
                messagePanel2.Visibility = Visibility.Hidden;
            });

            appStore.Subscribe<AutoColorGradingColorimetryResultChangedEvent>(r =>
            {
                var o = r.Result.Pixels;
                var ma = r.Result.MeasureArea;
                var p = DisplayPixels.Convert(o);
                var w = p.Width;
                var h = p.Height;
                var stride = w * 4;
                var bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                var len = stride * h;
                bmp.WritePixels(new Int32Rect(0, 0, w, h), p.Pix, stride, 0, 0);

                monitor.Source = bmp;
            });


            HwndSource source = null;

            Loaded += (s, e) =>
            {
                currentReport.Visibility = Visibility.Visible;
                prevReport.Visibility = Visibility.Collapsed;

                RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink | RawInputDeviceFlags.NoLegacy, new WindowInteropHelper(ViewController.Instance.MainWindow).Handle);


                source = HwndSource.FromHwnd(new WindowInteropHelper(ViewController.Instance.MainWindow).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            };

            Unloaded += (s, e) =>
            {
                RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);

                source.RemoveHook(new HwndSourceHook(WndProc));
            };

            void HandleKeyPress(int key)
            {
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
                //e.Handled = true;
                if (key == VK_F5)
                {
                    if (reportIndex >= 10)
                        return;

                    currentReports.Skip(reportIndex);
                    reportIndex++;
                    //if(_reportIndex >= 10)
                    //{
                    //    _reportIndex = 0;
                    //}

                    //captureImageList.Items.Clear();

                }
                if (key == VK_F7)
                {
                    currentReports.Copy(prevReports);
                    for (var i = 0; i < 10; i++)
                    {
                        currentReports.Remove(i);
                    }
                    reportIndex = 0;
                }
                if (key == VK_F6)
                {
                    if (reportIndex - 1 < 0)
                        return;

                    reportIndex--;
                    if (reportIndex < 0)
                    {
                        reportIndex = 0;
                    }

                    currentReports.Remove(reportIndex);
                }
                if (key == VK_F11)
                {
                    prevReport.Visibility = Visibility.Visible;
                    currentReport.Visibility = Visibility.Collapsed;
                }
                if (key == VK_F12)
                {
                    prevReport.Visibility = Visibility.Collapsed;
                    currentReport.Visibility = Visibility.Visible;
                }
            }

            appStore.Subscribe<AppAbortedEvent>(_ =>
            {
                autoGradingCancellationTokenSource?.Cancel();
                messagePanel1.Visibility = Visibility.Visible;
                messagePanel2.Visibility = Visibility.Hidden;
            });



            async void StartAutoGrading(string sn)
            {
                //if (sn.Length != 8)
                //{
                //    MessageBox.Show("バーコードの読み取りに失敗しました.");
                //    return;
                //}
                if (autogradingStarted)
                    return;

                autogradingStarted = true;
                var condition = appStore.ColorimetryCondition;
                var condition2 = appStore.ColorGradingCondition;
                serialNumber.Text = sn;
                messagePanel1.Visibility = Visibility.Collapsed;
                messagePanel2.Visibility = Visibility.Visible;
                monitor.Source = null;
                autoGradingCancellationTokenSource = new CancellationTokenSource();
                await autoGrading.ExecuteAsync(serialNumber.Text.Trim(), condition, condition2, autoGradingCancellationTokenSource);
                autogradingStarted = false;
            }



            IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                const int WM_INPUT = 0x00FF;
                const int WM_KEYDOWN = 0x0100;
                const int WM_KEYUP = 0x0101;
                const int WM_CHAR = 0x0102;

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
                // You can read inputs by processing the WM_INPUT message.
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
                                //{
                                //    var key = keyboard.Keyboard.VirutalKey;

                                //    if (key == VK_TAB)
                                //    {
                                //        barcode = barcode.Trim();
                                //        StartAutoGrading(barcode);
                                //        barcode = string.Empty;
                                //    }

                                //    if (key != VK_SHIFT)
                                //    {
                                //        barcode += ((char)keyboard.Keyboard.VirutalKey).ToString();
                                //    }
                                //}
                                //else
                                //{
                                //    var key = keyboard.Keyboard.VirutalKey;
                                //    HandleKeyPress(key);
                                //}
                                var key = keyboard.Keyboard.VirutalKey;

                                if (key == VK_TAB)
                                {
                                    barcode = barcode.Trim();
                                    if (barcode.Length != 8)
                                    {
                                        MessageBox.Show("Barcode reading failed.");
                                        barcode = string.Empty;
                                        break;
                                    }
                                    //if (!Char.IsLetter(barcode.Substring(0, 1).ToCharArray()[0]))
                                    //{
                                    //    MessageBox.Show("バーコードの読み取りに失敗しました.");
                                    //    break;
                                    //}
                                    StartAutoGrading(barcode);
                                    barcode = string.Empty;
                                    break;
                                }

                                if (key != VK_SHIFT)
                                {
                                    if ((key >= 0x30 && key <= 0x39) || (key >= 0x41 && key <= 0x5A))
                                    {
                                        barcode += ((char)keyboard.Keyboard.VirutalKey).ToString();
                                    }
                                }

                                HandleKeyPress(key);
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

    public class GradingReports
    {
        public TextBlock[] No { get; set; }
        public TextBlock[] Grade { get; set; }
        public TextBlock[] LCHH { get; set; }

        public GradingReports(TextBlock[] no, TextBlock[] grade, TextBlock[] lchh)
        {
            No = no;
            Grade = grade;
            LCHH = lchh;
        }

        public void Change(int index, string no, string grade, string lchh)
        {
            try
            {
                No[index].Text = no;
                Grade[index].Text = grade;
                LCHH[index].Text = lchh;
                LCHH[index].Background = grade.EndsWith("↑") ? Brushes.DarkBlue : grade.EndsWith("↓") ? Brushes.DarkRed : Brushes.Transparent;
            }
            catch
            {

            }
        }

        public void Remove(int index)
        {
            try
            {
                No[index].Text = string.Empty;
                Grade[index].Text = string.Empty;
                LCHH[index].Text = string.Empty;
                LCHH[index].Background = Brushes.Transparent;
            }
            catch
            {

            }
        }
        public void RemoveAll()
        {
            for (var i = 0; i < 10; i++)
            {
                Remove(i);
            }
        }
        public void Skip(int index)
        {
            try
            {
                No[index].Text = "-----";
                Grade[index].Text = "-";
                LCHH[index].Text = "---";
                LCHH[index].Background = Brushes.Transparent;
            }
            catch
            {

            }
        }

        public void Copy(GradingReports obj)
        {
            for (var i = 0; i < 10; i++)
            {
                obj.No[i].Text = No[i].Text;
                obj.Grade[i].Text = Grade[i].Text;
                obj.LCHH[i].Text = LCHH[i].Text;
                obj.LCHH[i].Background = LCHH[i].Background;
            }
        }
    }

}
