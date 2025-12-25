using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace CGLCMIV2App
{
    /// <summary>
    /// Configure.xaml の相互作用ロジック
    /// </summary>
    public partial class Configuration : UserControl
    {
        //CGLDCG.AppContext app = CGLDCG.AppContext.Instance;
        public Configuration(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            Loaded += delegate
            {
                var appStore = serviceProvider.GetService<AppStore>();

                var c = appStore.ColorimetryCondition;
                var e = c.ExposureTime;
                var integ = c.Integration;
                var l = c.LEDValue;
                var wp = appStore.ColorimetryCondition.Whitepoint;
                txtWhitepoint.Text = $"{wp} ({wp.ToYxy()})";
                txtmeasureColorExposureTime.Text = $"{e} msec";
                txtmeasureColorIntegration.Text = integ.ToString();
                lValue1.Text = l.D65Value.ToString();
                lValue2.Text = l.UVValue.ToString();
                txtVersion1.Text = appStore.SystemVersion;
                txtVersion2.Text = appStore.SystemVersion;
                txtMesLogPath.Text = appStore.MeasureResultOutputPath;
            };

            Unloaded += delegate
            {
                EventBus.EventBus.Instance.Publish(new ChangeMeasureResultOutputPath(txtMesLogPath.Text.Trim()));
            };
        }
    }
}
