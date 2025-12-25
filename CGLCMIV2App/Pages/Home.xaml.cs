using CGLCMIV2.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
using Microsoft.Extensions.DependencyInjection;

namespace CGLCMIV2App
{
    /// <summary>
    /// Home.xaml の相互作用ロジック
    /// </summary>
    public partial class Home : UserControl
    {
        
        public Home(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var appStore = serviceProvider.GetService<AppStore>();
            appStore.Subscribe<AppHardwareDisConnectedEvent>(_ => 
            {
                MenuMeasureAuto.IsEnabled = false;
                MenuMeasureManual.IsEnabled = false;
                //MenuConfiguration.IsEnabled = false;
                MenuCalibrate.IsEnabled = false;
            });

            appStore.Subscribe<AppHardwareConnectedEvent>(_ =>
            {
                MenuMeasureAuto.IsEnabled = false;
                MenuMeasureManual.IsEnabled = false;
                //MenuConfiguration.IsEnabled = true;
                MenuCalibrate.IsEnabled = true;
            });

            appStore.Subscribe<AppStartedEvent>(_ =>
            {
                MenuMeasureAuto.IsEnabled = true;
                MenuMeasureManual.IsEnabled = true;
                //MenuConfiguration.IsEnabled = true;
                MenuCalibrate.IsEnabled = true;
            });

            appStore.Subscribe<AppStoppedEvent>(_ =>
            {
                MenuMeasureAuto.IsEnabled = false;
                MenuMeasureManual.IsEnabled = false;
                //MenuConfiguration.IsEnabled = true;
                MenuCalibrate.IsEnabled = true;
            });
            
            Loaded += (s, e) =>
            {
                SelectPage.SelectedIndex = -1;
                
            };

            Unloaded += (s, e) =>
            {
                
            };
            SelectPage.SelectionChanged += (s, e) =>
            {
                //var w = ViewController.Instance.MainWindow;
                //switch(SelectPage.SelectedIndex)
                //{
                //    case 0:
                //        w.content1.Content = ViewController.Instance.Get("grading");
                //        break;
                //    case 1:
                //        w.content1.Content = ViewController.Instance.Get("measuring");
                //        break;
                //    case 2:
                //        w.content1.Content = ViewController.Instance.Get("configuration");
                //        break;
                //    case 3:
                //        w.content1.Content = ViewController.Instance.Get("calibration");
                //        break;
                //}

                //w.listViewPage.SelectedIndex = -1;
                switch (SelectPage.SelectedIndex)
                {
                    case 0:
                        ViewController.Instance.Chanage(ViewController.CONTENT_COLORGRADING);
                        break;
                    case 1:
                        ViewController.Instance.Chanage(ViewController.CONTENT_COLORMEASURING);
                        break;
                    case 2:
                        ViewController.Instance.Chanage(ViewController.CONTENT_CONFIGURATION);
                        break;
                    case 3:
                        ViewController.Instance.Chanage(ViewController.CONTENT_CALIBRATION);
                        break;
                }

            };
        }
    }
}
