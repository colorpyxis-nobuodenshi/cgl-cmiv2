using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Application
{
    public interface IAppSettings
    {
        public string SystemName { get; }
        public string SystemModel { get; }
        public string SystemVersion { get; }
        public string SystemSerialNumber { get; }
    }
    public class AppSettings
    {
        public string SystemName { get; set; }
        public string SystemModel { get; set; }
        public string SystemVersion { get; set; }
        public string AppVersion { get; set; }
        public string SerialNumber { get; set; }
        public string LatestDate { get; set; }
        public CalibrationLatestDate CalibrationLatestDate { get; set; }
        public MeasureCondition MeasureCondition { get; set; }
        public MeasureResultOutputOption MeasureResultOutputOption { get; set; }
    }
    public class CalibrationLatestDate
    {
        public string Whitepoint { get; set; }
        public string Opticalpower { get; set; }
        public string CameraShading { get; set; }
    }
    public class MeasureCondition
    {
        public bool AutoCalibrateStartup { get; set; }
        public bool AutoCalibrateWhitepoint { get; set; }
        public double[][] Matrix { get; set; }
        public int ExposureTime { get; set; }
        public int Integration { get; set; }
        public int MeasurePoint { get; set; }
        public string ShadingPixels { get; set; }
        public int[] LEDValues { get; set; }
        public double[] Whitepoint { get; set; }
        public double[] WhitepointForCorrect { get; set; }
        public double Opticalpower { get; set; }
        public string Colorgrading { get; set; }
        public int[] DefaultLEDValues { get; set; }
        public double[] DefaultWhitepoint { get; set; }
        public double[] DefaultWhitepointForCorrect { get; set; }
        public double OpticalpowerFactor { get; set; }
    }

    //public class Display
    //{
    //    public string Colorspace { get; set; }
    //    public int[] GraphRange { get; set; }
    //    public bool GraphAutoScale { get; set; }
    //}

    public class MeasureResultOutputOption
    { 
        public string OutputPath { get; set; }
        public bool ColorGradingResultOutput { get; set; }
        public bool ColorGradingPixelsOutput { get; set; }
        public bool ColorMeasuringResultOutput { get; set; }
        public bool ColorMeasuringPixelsOutput { get; set; }
    }
}
