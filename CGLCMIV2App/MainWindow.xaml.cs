using CGLCMIV2.Application;
using CGLCMIV2.Domain;
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
using System.Windows.Interop;

namespace CGLCMIV2App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var measureAutoPage = new MeasureAuto(serviceProvider);
            var measurePage = new Measure(serviceProvider);
            var configuratonPage = new Configuration(serviceProvider);
            var calibratoinPage = new Calibration(serviceProvider);
            var home = new Home(serviceProvider);

            var appStore = serviceProvider.GetService<AppStore>();
            var messageWindow = serviceProvider.GetService<IMessageWindow>();

            var connected = false;
            var started = false;

            //Loaded += (s, e) =>
            //{
            //    var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //    source.AddHook(new HwndSourceHook(WndProc));
            //};

            Closing += async (s, e) =>
            {
                if(MessageBox.Show("\r\nDo you want to close the application?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    //MessageBox.Show("装置が運転中です。停止してから終了してください。");
                    //e.Cancel = true;
                    if(started)
                    {
                        await serviceProvider.GetService<Stop>().ExecuteAsync();
                    }
                    if(connected)
                    {
                        await serviceProvider.GetService<DisconnectHardware>().ExecuteAsync();
                    }
                    
                    return;
                }
                e.Cancel = true;

            };
            listViewPage.SelectionChanged += (s, e) =>
            {
                switch (listViewPage.SelectedIndex)
                {
                    case 0:
                        ViewController.Instance.Chanage(ViewController.CONTENT_HOME);
                        break;
                }
                listViewPage.SelectedIndex = -1;
            };


            btnPreStart.Click += async delegate
            {
                messageWindow = serviceProvider.GetService<IMessageWindow>();
                messageWindow?.Show("Hardware connection\r\nChecking hardware.", this);
                await serviceProvider.GetService<ConnectHardware>().ExecuteAsync();
                messageWindow?.Close();
                
            };
            btnPreStop.Click += async delegate
            {
                await serviceProvider.GetService<DisconnectHardware>().ExecuteAsync();
                
            };
            btnStart.Click += async delegate
            {
                messageWindow = serviceProvider.GetService<IMessageWindow>();
                messageWindow?.Show("Calibrating the white point", this);
                var c = serviceProvider.GetService<AppStore>().ColorimetryCondition;
                await serviceProvider.GetService<Start>().ExecuteAsync(c);
                messageWindow?.Close();
                
            };
            btnStop.Click += async delegate
            {
                if(MessageBox.Show("Do you want to stop?", "",MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    await serviceProvider.GetService<Stop>().ExecuteAsync();
                    
                }
            };
            btnReset.Click += async delegate
            {
                await serviceProvider.GetService<Reset>().ExecuteAsync();
                
            };
            btnAbort.Click += async delegate
            {
                if (MessageBox.Show("\r\nDo you want to cancel measurement?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    btnAbort.IsEnabled = false;
                    //messageWindow = serviceProvider.GetService<IMessageWindow>();
                    //messageWindow?.Show("測定を中止しています", this);
                    await serviceProvider.GetService<Abort>().ExecuteAsync();
                    //messageWindow?.Close();
                    btnAbort.IsEnabled = true;
                }
            };


            appStore.Subscribe<OperatingTimeCountEvent>(a => 
            {
                txtKadoujikan.Text = a.Count.ToString();
            });
            appStore.Subscribe<TemperatureChangedEvent>(a =>
            {
                txtRoomTemperature.Text = a.Temperature.ToString("F2");
            });
            appStore.Subscribe<OpticalpowerChangedEvent>(a =>
            {
                txtOpticalpower.Text = a.Opticalpower.ToString("F2");
            });
            appStore.Subscribe<AppHardwareConnectedEvent>(_ =>
            {
                Status.Background = Brushes.DarkGreen;
                txtKaishijikan.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                btnPreStart.IsEnabled = true;
                btnPreStop.IsEnabled = true;
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = true;
                btnReset.IsEnabled = false;
                btnAbort.IsEnabled = false;
                btnPreStart.Visibility = Visibility.Hidden;
                btnPreStop.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Hidden;
                connected = true;
            });
            appStore.Subscribe<AppHardwareDisConnectedEvent>(_ =>
            {
                Status.Background = Brushes.Gray;
                btnPreStart.IsEnabled = true;
                btnPreStop.IsEnabled = true;
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = false;
                btnReset.IsEnabled = false;
                btnAbort.IsEnabled = false;
                btnPreStart.Visibility = Visibility.Visible;
                btnPreStop.Visibility = Visibility.Hidden;
                btnStart.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Hidden;

                ViewController.Instance.Chanage(ViewController.CONTENT_HOME);
                connected = false;
            });

            appStore.Subscribe<AppStartedEvent>(_ =>
            {
                Status.Background = Brushes.Lime;
                btnPreStart.IsEnabled = false;
                btnPreStop.IsEnabled = false;
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = true;
                btnReset.IsEnabled = false;
                btnAbort.IsEnabled = true;
                btnPreStop.IsEnabled = false;
                btnPreStart.Visibility = Visibility.Hidden;
                btnPreStop.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Hidden;
                btnStop.Visibility = Visibility.Visible;
                started = true;
            });

            appStore.Subscribe<AppStoppedEvent>(_ =>
            {
                Status.Background = Brushes.DarkGreen;
                btnPreStart.IsEnabled = true;
                btnPreStop.IsEnabled = true;
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = true;
                btnReset.IsEnabled = false;
                btnAbort.IsEnabled = false;
                btnPreStop.IsEnabled = true;
                btnPreStart.Visibility = Visibility.Hidden;
                btnPreStop.Visibility = Visibility.Visible;
                btnStart.Visibility = Visibility.Visible;
                btnStop.Visibility = Visibility.Hidden;

                ViewController.Instance.Chanage(ViewController.CONTENT_HOME);

                started = false;
            });

            //appStore.Subscribe<AppResetedEvent>(_ =>
            //{
            //    Status.Background = Brushes.Gray;
            //    btnPreStart.IsEnabled = true;
            //    btnStart.IsEnabled = false;
            //    btnReset.IsEnabled = false;
            //    btnAbort.IsEnabled = false;
            //    btnPreStart.Visibility = Visibility.Visible;
            //    btnPreStop.Visibility = Visibility.Hidden;
            //    btnStart.Visibility = Visibility.Visible;
            //    btnStop.Visibility = Visibility.Hidden;

            //    ViewController.Instance.Chanage(ViewController.CONTENT_HOME);
            //});

            appStore.Subscribe<AppErrorEvent>(a =>
            {
                messageWindow?.Close();

                Status.Background = Brushes.Red;
                btnPreStart.IsEnabled = false;
                btnStart.IsEnabled = false;
                btnReset.IsEnabled = true;
                btnAbort.IsEnabled = false;
                btnPreStop.IsEnabled = false;
                btnStop.IsEnabled = false;

                if (a is AppHardwareConnectedFailureEvent)
                {
                    MessageBox.Show($"An error occurred in the hardware connection. Check the power supply of the device and the connection cable.\r\n{a.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if(a is AutoGradingFailureEvent)
                {
                    MessageBox.Show($"An application error occurred during color grading measurement. \r\nPlease \"Reset\".\r\n{a.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (a is AutoColorMeasureFailureEvent)
                {
                    MessageBox.Show($"An application error occurred during color measurement. \r\nPlease \"Reset\".\r\n{a.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (a is AppAbortedFailureEvent)
                {
                    MessageBox.Show($"An application error occurred during cancellation processing. \r\nPlease \"Reset\".\r\n{a.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show($"An application error has occurred. \r\nPlease \"Reset\".\r\n{a.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);

            });

            appStore.Subscribe<AppMode>(a =>
            {
                switch(a)
                {
                    case AppModeColorGrading:
                    case AppModeColorMeasuring:
                        //btnAbort.IsEnabled = true;
                        btnAbort.Visibility = Visibility.Visible;
                        break;
                    default:
                        //btnAbort.IsEnabled = false;
                        btnAbort.Visibility = Visibility.Hidden;
                        break;
                }
            });

            ViewController.Instance.Add(ViewController.CONTENT_COLORGRADING, measureAutoPage);
            ViewController.Instance.Add(ViewController.CONTENT_COLORMEASURING, measurePage);
            ViewController.Instance.Add(ViewController.CONTENT_CALIBRATION, calibratoinPage);
            ViewController.Instance.Add(ViewController.CONTENT_CONFIGURATION, configuratonPage);
            ViewController.Instance.Add(ViewController.CONTENT_HOME, home);
            ViewController.Instance.MainWindow = this;

            ViewController.Instance.Chanage(ViewController.CONTENT_HOME);
        }

        //static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    const int WM_INPUT = 0x00FF;
        //    if(msg == WM_INPUT)
        //    {
        //        handled = true;
        //    }
        //    return IntPtr.Zero;
        //}
    }

    
}
