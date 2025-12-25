using CGLCMIV2.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CGLCMIV2App
{
    public class ViewController
    {
        static ViewController _instance;

        Dictionary<string, UserControl> _views = new Dictionary<string, UserControl>();

        public static string CONTENT_HOME = "home";
        public static string CONTENT_COLORGRADING = "colorgrading";
        public static string CONTENT_COLORMEASURING = "colormeasuring";
        public static string CONTENT_CONFIGURATION = "configuration";
        public static string CONTENT_CALIBRATION = "calibration";
        public static ViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ViewController();

                return _instance;
            }
        }

        ViewController()
        {

        }

        public void Add(string name, UserControl control)
        {
            _views.Add(name, control);
        }

        //public UserControl Get(string name)
        //{
        //    Current = _views[name];
        //    if(name == CONTENT_HOME)
        //    {
        //        EventBus.EventBus.Instance.Publish(new AppModeHome());
        //    }
        //    if (name == CONTENT_COLORGRADING)
        //    {
        //        EventBus.EventBus.Instance.Publish(new AppModeColorGrading());
        //    }
        //    if (name == CONTENT_COLORMEASURING)
        //    {
        //        EventBus.EventBus.Instance.Publish(new AppModeColorMeasuring());
        //    }
        //    if (name == CONTENT_CALIBRATION)
        //    {
        //        EventBus.EventBus.Instance.Publish(new AppModeCalibration());
        //    }
        //    if (name == CONTENT_CONFIGURATION)
        //    {
        //        EventBus.EventBus.Instance.Publish(new AppModeConfiguration());
        //    }

        //    return _views[name];
        //}

        public void Chanage(string name)
        {

            MainWindow.content1.Content = _views[name];
            if (name == CONTENT_HOME)
            {
                EventBus.EventBus.Instance.Publish(new AppModeHome());
            }
            if (name == CONTENT_COLORGRADING)
            {
                EventBus.EventBus.Instance.Publish(new AppModeColorGrading());
            }
            if (name == CONTENT_COLORMEASURING)
            {
                EventBus.EventBus.Instance.Publish(new AppModeColorMeasuring());
            }
            if (name == CONTENT_CALIBRATION)
            {
                EventBus.EventBus.Instance.Publish(new AppModeCalibration());
            }
            if (name == CONTENT_CONFIGURATION)
            {
                EventBus.EventBus.Instance.Publish(new AppModeConfiguration());
            }
            MainWindow.listViewPage.SelectedIndex = -1;
        }

        public MainWindow MainWindow
        {
            get;
            set;
        }

        //public UserControl Current
        //{
        //    get;
        //    private set;
        //}
    }
}
