using CGLCMIV2.Application;
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
using System.Threading;
using CGLCMIV2.Domain;
using System.Drawing;
using System.Drawing.Imaging;

namespace CGLCMIV2App
{
    /// <summary>
    /// Calibration.xaml の相互作用ロジック
    /// </summary>
    public partial class Calibration : UserControl
    {
        public Calibration(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var cameraCaptureStart = serviceProvider.GetService<CameraCaptureStart>();
            var cameraCaptureStop = serviceProvider.GetService<CameraCaptureStop>();
            var cameraCaptureSnap = serviceProvider.GetService<CameraCaptureSnap>();
            var tiffStore = serviceProvider.GetService<IPixelsTiffFileStore>();
            var scanWhitepoint = serviceProvider.GetService<ScanWhitepoint>();
            var makeShadingData = serviceProvider.GetService<MakeShadingData>();
            var ledPowerChange = serviceProvider.GetService<LEDPowerChange>();
            var messageWindow = serviceProvider.GetService<IMessageWindow>();

            var appStore = serviceProvider.GetService<AppStore>();
            var condition = appStore.ColorimetryCondition;
            var ledValue = condition.LEDValue;
            txtLEDD65.Text = ledValue.D65Value.ToString();
            txtLEDUV.Text = ledValue.UVValue.ToString();
            CalibrateDateLED.Text = appStore.CalibrateLatestDate.Opticalpower;
            CalibrateDateShading.Text = appStore.CalibrateLatestDate.CameraShading;
            var wp = condition.Whitepoint;
            var wpc = condition.WhitepointForCorrect;
            CalibrateWhitepoint.Text = $"{wp.ToString()}   ({wpc.ToString()})";
            CalibrateWhitepointYxy.Text = $"{wp.ToYxy().ToString()}   ({wpc.ToYxy().ToString()})";
            CalibrateDateWhitepoint.Text = appStore.CalibrateLatestDate.Whitepoint;
            btnGetWhitepoint.Click += async delegate
            {
                btnGetWhitepoint.IsEnabled = false;
                messageWindow = serviceProvider.GetService<IMessageWindow>();
                messageWindow?.Show("Getting the white point.", ViewController.Instance.MainWindow);
                await scanWhitepoint.ExecuteAsync(appStore.ColorimetryCondition);
                messageWindow?.Close();
                btnGetWhitepoint.IsEnabled = true;
            };
            btnMakeShadingCorrectPixels.Click += async delegate
            {
                if(MessageBox.Show("Do you want to create sensitivity unevenness correction data?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    btnMakeShadingCorrectPixels.IsEnabled = false;
                    messageWindow = serviceProvider.GetService<IMessageWindow>();
                    messageWindow?.Show("Creating sensitivity unevenness correction data\r\nPlease wait for a while.", ViewController.Instance.MainWindow);
                    await makeShadingData.ExecuteAsync(appStore.ColorimetryCondition);
                    messageWindow?.Close();
                    btnMakeShadingCorrectPixels.IsEnabled = true;
                }
            };
            btnAutoStageOrg.Click += async delegate
            {
                btnAutoStageOrg.IsEnabled = false;
                await serviceProvider.GetService<AutoStageMoveORG>().ExecuteAsync();
                btnAutoStageOrg.IsEnabled = true;
            };
            btnAutoStageHome.Click += async delegate
            {
                btnAutoStageHome.IsEnabled = false;
                await serviceProvider.GetService<AutoStageMoveHome>().ExecuteAsync();
                btnAutoStageHome.IsEnabled = true;
            };
            
            btnAutoStageMeasurepoint.Click += async delegate
            {
                btnAutoStageMeasurepoint.IsEnabled = false;
                await serviceProvider.GetService<AutoStageMoveMeasurePoint>().ExecuteAsync();
                btnAutoStageMeasurepoint.IsEnabled = true;
            };
            btnAutoStageWorksetpoint.Click += async delegate
            {
                btnAutoStageWorksetpoint.IsEnabled = false;
                await serviceProvider.GetService<AutoStageMoveWorkSetPoint>().ExecuteAsync();
                btnAutoStageWorksetpoint.IsEnabled = true;
            };
            btnAutoStageRotateCW45.Click += async delegate 
            {
                btnAutoStageRotateCW45.IsEnabled = false;
                await serviceProvider.GetService<AutoStageRotateCW45>().ExecuteAsync();
                btnAutoStageRotateCW45.IsEnabled = true;
            };
            btnAutoStageRotateCCW45.Click += async delegate
            {
                btnAutoStageRotateCCW45.IsEnabled = false;
                await serviceProvider.GetService<AutoStageRotateCCW45>().ExecuteAsync();
                btnAutoStageRotateCCW45.IsEnabled = true;
            };
            btnAutoStageRotateHome.Click += async delegate
            {
                btnAutoStageRotateHome.IsEnabled = false;
                await serviceProvider.GetService<AutoStageRotateHome>().ExecuteAsync();
                btnAutoStageRotateHome.IsEnabled = true;
            };
            var cameraCaptureStopTokenSource = new CancellationTokenSource();
            //svwCameraPixels.ScrollToVerticalOffset(768 / 4);
            //svwCameraPixels.ScrollToHorizontalOffset(1024 / 4);
           
            btnCameraLiveStart.Click += delegate 
            {
                btnCameraLiveStart.IsEnabled = false;
                btnCameraLiveStop.IsEnabled = true;
                cameraCaptureStopTokenSource = new CancellationTokenSource();
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
                        imgCameraPixels.Source = bmp;
                        imgCameraPixels.Stretch = Stretch.Fill;
                        //imgCameraPixels.Width = w;
                        //imgCameraPixels.Height = h;
                    });
                }, cameraCaptureStopTokenSource, 1);
            };
            btnCameraLiveStop.Click += delegate
            {
                btnCameraLiveStart.IsEnabled = true;
                btnCameraLiveStop.IsEnabled = false;
                cameraCaptureStop.Execute(cameraCaptureStopTokenSource);
            };
            btnCameraSnap.Click += async delegate 
            {
                btnCameraSnap.IsEnabled = false;
                var token = new CancellationTokenSource();
                var p = await cameraCaptureSnap.ExecuteAsync(token, 1);

                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Tiff";
                dlg.DefaultExt = ".tif";
                dlg.Filter = "xyz pixels (.tif)|*.tif";

                if (dlg.ShowDialog() == true)
                {

                    string filename = dlg.FileName;

                    await tiffStore.ExecuteAsync(filename, p);
                }
                btnCameraSnap.IsEnabled = true;
            };
            btnLEDSet.Click += delegate 
            {
                var d65 = int.Parse(txtLEDD65.Text.Trim());
                var uv = int.Parse(txtLEDUV.Text.Trim());

                ledPowerChange.Execute(d65, uv);
            };
            btnLEDDefault.Click += delegate
            {
                txtLEDD65.Text = appStore.DefaultLEDValues.D65.ToString();
                txtLEDUV.Text = appStore.DefaultLEDValues.UV.ToString();
            };

            Loaded += delegate
            {
                cameraCaptureStopTokenSource = new CancellationTokenSource();
                btnCameraLiveStart.IsEnabled = true;
                btnCameraLiveStop.IsEnabled = false;
            };

            Unloaded += delegate
            {
                cameraCaptureStopTokenSource?.Cancel(true);
            };

            appStore.Subscribe<ScanWhitepointCompletedEvent>(a =>
            {
            CalibrateWhitepoint.Text = $"{a.Whitepoint}";// ({a.WhitepointForCorrection})";
            CalibrateWhitepointYxy.Text = $"{a.Whitepoint.ToYxy()}";// ({a.WhitepointForCorrection.ToYxy()})";
                CalibrateDateWhitepoint.Text = a.ProcessingDatetime;
            });
            appStore.Subscribe<ChangeLEDPowerCompletedEvent>(a =>
            {
                CalibrateDateLED.Text = a.ProcessingDate;
            });
            appStore.Subscribe<MakeShadingCorrectDataCompletedEvent>(a =>
            {
                CalibrateDateShading.Text = a.ProcessingDatetime;
            });
        }

    }
}
