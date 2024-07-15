using CGLCMIV2.Application;
using CGLCMIV2.Domain;
using CGLCMIV2.Infrastructure;
using System;
using Windows.UI.WebUI;

namespace CGLCMIV2App
{
    public class AppStore
    {
        public AppStore(AppSettings settings, IPixelsTiffFileLoader loader1, IColorGradingConditonFileLoader loader2, IInstrumentalErrorCorrectionMatrixFileLoader loader3)
        {
            var c = settings.MeasureCondition;
            var shd = loader1.Execte(c.ShadingPixels);
            var cgc = loader2.Execute(c.Colorgrading);
            var w1 = new CIEXYZ(c.Whitepoint[0], c.Whitepoint[1], c.Whitepoint[2]);
            var w2 = new CIEXYZ(c.WhitepointForCorrect[0], c.WhitepointForCorrect[1], c.WhitepointForCorrect[2]);
            var w3 = new CIEXYZ(c.WhitepointOnSpectralon[0], c.WhitepointOnSpectralon[1], c.WhitepointOnSpectralon[2]);
            var w4 = new CIEXYZ(c.WhitepointForCorrectOnSpectralon[0], c.WhitepointForCorrectOnSpectralon[1], c.WhitepointForCorrectOnSpectralon[2]);
            var l = (c.LEDValues[0], c.LEDValues[1]);
            var labm = loader3.Execute("kisa_matrix.json")[settings.SerialNumber];
            var mccm = MultiColorConversionMatrix.Load(c.MultiColorConversionMatrix);
            ColorimetryCondition = ColorimetryCondition.Create(c.ExposureTime, c.Integration, shd, mccm, w1, w2, l, c.MeasurePoint, labm, w3, w4, c.WhitebalanceGain);
            ColorGradingCondition = cgc;
            CalibrateLatestDate = settings.CalibrationLatestDate;
            AppSettings = settings;
            AppSettings.LatestDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            EventBus.EventBus.Instance.Subscribe<ScanWhitepointCompletedEvent>(a => 
            {
                var c = ColorimetryCondition;
                ColorimetryCondition = ColorimetryCondition.Create(c.ExposureTime, c.Integration, c.ShadingCorrectPixels, c.MultiColorConversionMatrix, a.Whitepoint, a.WhitepointCorner, c.LEDValue, c.MeasurePoint, labm, c.WhitepointOnSpectralon, c.WhitepointForCorrectionOnSpectralon, c.WhitebalanceGain);
                AppSettings.MeasureCondition.Whitepoint = a.Whitepoint.To1DArray();
                AppSettings.MeasureCondition.WhitepointForCorrect = a.WhitepointCorner.To1DArray();
                AppSettings.CalibrationLatestDate.Whitepoint = a.ProcessingDatetime;
            });
            EventBus.EventBus.Instance.Subscribe<ScanWhitepointOnSpectralonCompletedEvent>(a =>
            {
                var c = ColorimetryCondition;
                ColorimetryCondition = ColorimetryCondition.Create(c.ExposureTime, c.Integration, c.ShadingCorrectPixels, c.MultiColorConversionMatrix, c.Whitepoint, c.WhitepointForCorrect, c.LEDValue, c.MeasurePoint, labm, a.Whitepoint, a.WhitepointCorner, c.WhitebalanceGain);
                AppSettings.MeasureCondition.Whitepoint = a.Whitepoint.To1DArray();
                AppSettings.MeasureCondition.WhitepointForCorrect = a.WhitepointCorner.To1DArray();
                AppSettings.CalibrationLatestDate.Whitepoint = a.ProcessingDatetime;
            });
            EventBus.EventBus.Instance.Subscribe<MakeShadingCorrectDataCompletedEvent>(a =>
            {
                var c = ColorimetryCondition;
                ColorimetryCondition = ColorimetryCondition.Create(c.ExposureTime, c.Integration, a.ShadingCorrectPixels, c.MultiColorConversionMatrix, c.Whitepoint, c.WhitepointForCorrect, c.LEDValue, c.MeasurePoint, labm, c.WhitepointOnSpectralon, c.WhitepointForCorrectionOnSpectralon, c.WhitebalanceGain);
            });
            EventBus.EventBus.Instance.Subscribe<ChangeLEDPowerCompletedEvent>(a =>
            {
                var c = ColorimetryCondition;
                ColorimetryCondition = ColorimetryCondition.Create(c.ExposureTime, c.Integration, c.ShadingCorrectPixels, c.MultiColorConversionMatrix, c.Whitepoint, c.WhitepointForCorrect, (a.D65Power, a.UVPower), c.MeasurePoint, labm, c.WhitepointOnSpectralon, c.WhitepointForCorrectionOnSpectralon, c.WhitebalanceGain);
                AppSettings.MeasureCondition.LEDValues = [a.D65Power, a.UVPower ];
                AppSettings.CalibrationLatestDate.Opticalpower = a.ProcessingDate;
            });
            EventBus.EventBus.Instance.Subscribe<ChangeMeasureResultOutputPath>(a =>
            {
                AppSettings.MeasureResultOutputOption.OutputPath = a.MeasureResultOutputPath;
            });
        }

        public ColorimetryCondition ColorimetryCondition { get; private set; }
        public ColorGradingCondition ColorGradingCondition { get; }
        public CalibrationLatestDate CalibrateLatestDate { get; private set; }
        public string SystemVersion { 
            get
            {
                return AppSettings.SystemVersion;
            }
        }
        public string SystemSerialNumber
        {
            get
            {
                return AppSettings.SerialNumber;
            }
        }
        public string SystemName
        {
            get
            {
                return AppSettings.SystemName;
            }
        }
        public string SystemModel
        {
            get
            {
                return AppSettings.SystemModel;
            }
        }
        public (int D65, int UV) DefaultLEDValues
        {
            get
            {
                var v = AppSettings.MeasureCondition.DefaultLEDValues;
                return (v[0], v[1]);
            }
        }
        public string MeasureResultOutputPath
        { 
            get
            {
                return AppSettings.MeasureResultOutputOption.OutputPath;
            }
        }
        AppSettings AppSettings { get; }
        public void Store()
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(AppSettings, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); ; ;
            System.IO.File.Copy("appsettings.json", "appsettings.1.json", true);
            System.IO.File.WriteAllText("appsettings.json", jsonString);
        }

        public IDisposable Subscribe<T>(Action<T> action) where T : EventBus.IEvent
        {

            return EventBus.EventBus.Instance.Subscribe(action);
        }
    }
    
    //public class EventDispatcher
    //{
    //    public void Publish<T>(T @event) where T : EventBus.IEvent
    //    {
    //        EventBus.EventBus.Instance.Publish(@event);
    //    }
    //    public IDisposable Subscribe<T>(Action<T> action) where T : EventBus.IEvent
    //    {

    //        return EventBus.EventBus.Instance.Subscribe(action);
    //    }
    //}
}
