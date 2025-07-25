using CGLCMIV2.Domain;
using EventBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Application
{
       
    public class AppLifeCycle
    {
        public AppLifeCycle()
        {
            
        }
        public void Run()
        {
            EventBus.EventBus.Instance.Publish(new AppHardwareDisConnectedEvent());
        }
    }
    //public class ColorGradeStore
    //{
    //    public ColorGradeStore()
    //    {

    //    }
    //    public static ColorGradeStore Instance { get; } = new ColorGradeStore();

    //    public List<ColorGradeReport> ColorGradeReports { get; private set; }

    //    public void Store(ColorGradeReport report)
    //    {
    //        ColorGradeReports.Add(report);
    //    }
    //}
    //public class ColorMeasuringStore
    //{
    //    public ColorMeasuringStore()
    //    {

    //    }
    //    public static ColorMeasuringStore Instance { get; } = new ColorMeasuringStore();

    //    public List<ColorimetryReport> ColorimetryReports { get; private set; }

    //    public void Store(ColorimetryReport report)
    //    {
    //        ColorimetryReports.Add(report);
    //    }
    //}

    //public class AppSettingsStore
    //{
    //    public AppSettingsStore()
    //    {

    //    }
    //}

    //public class AppStateStore
    //{
    //    public AppStateStore()
    //    {

    //    }

    //    public AppStatus AppStatus { get; private set; }
    //    public AppMode AppMode { get; private set; }

    //}


    //public enum AppEventType
    //{
    //    ConnectingDevice,
    //    ConnectedDevice,
    //    DisConnectingDevice,
    //    DisConnectedDevice,
    //    Starting,
    //    Started,
    //    Stopping,
    //    Stopped,
    //    Reseting,
    //    Reseted,
    //    Fatal,
    //    Warn,

    //}
    //public enum AppMode
    //{
    //    None,
    //    AutoGrading,
    //    ColorMeasuring,
    //    Config
    //}
    //public enum AppStatus
    //{
    //    Connecting,
    //    Disconnecting,
    //    Disconnected,
    //    Stopping,
    //    Stopped,
    //    Ready,
    //    Run,
    //    Starting,
    //    MeasureGradeingReady,
    //    MeasureGradeingBusy,
    //    MeasureAutoManuaReady,
    //    MeasureAutoManuaBusy,
    //    SettingsReady,
    //    Error,
    //}

    public enum AutoGradingStatus
    {
        Stopped,
        Started,
        MovedWorkPoint,
        MovedHome,
        Rotated,
        Measured,
        Analyzed,
        Completed,
    }
    public enum ColorMeasuringStatus
    {
        Stopped,
        Started,
        Measured,
        MoveWorkPoint,
        MoveHome,
        Rotate,
        Error,
    }

    public class DeviceMonitorService
    {
        ILEDLight _led;
        ILogger _logger;
        double _opticalPowerFactor = 106.09 / 5470;
        public DeviceMonitorService(ILEDLight led, ILogger logger, AppSettings appSettings)
        {
            _led = led;
            _logger = logger;
            _opticalPowerFactor = appSettings.MeasureCondition.OpticalpowerFactor;
        }
        public void Run()
        {
            var count = 0L;
            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(observer =>
                {
                    EventBus.EventBus.Instance.Publish(new OperatingTimeCountEvent((count += 1) / 60));

                    //if (_appLife.Lateast is not AppHardwareDisConnectedEvent)
                    {
                        try
                        {
                            if (_led.IsConnected)
                            {
                                _led.ReadStatus();
                                var temp = _led.GetTemperature();
                                //var op = _led.GetOpticalPower() * _opticalPowerFactor;
                                EventBus.EventBus.Instance.Publish(new TemperatureChangedEvent(temp));
                                //EventBus.EventBus.Instance.Publish(new OpticalpowerChangedEvent(op));
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.Error(ex, "device monitoring error.");
                            throw;
                        }
                    }
                });
        }
    }
    public class FileOutputService
    {
        ILogger _logger;
        IMeasureResultWriter _measureResultWriter;
        IWhitepointWriter _whitepointWriter;
        readonly AppSettings _appSettings;
        public FileOutputService(ILogger logger, IMeasureResultWriter measureResultWriter, IWhitepointWriter whitepointWriter, AppSettings appSettings)
        {
            _logger = logger;
            _measureResultWriter = measureResultWriter;
            _whitepointWriter = whitepointWriter;
            _appSettings = appSettings;
        }
        public void Run()
        {
            var systemSerialNumber = _appSettings.SerialNumber;
            var option = _appSettings.MeasureResultOutputOption;

            //_measureResultWriter.CreateOptputDirectoryAndHeader("Serial Number,Color Grade,D65,L*,C*,h*,X,Y,Z,White,X,Y,Z,System ID,Measure Date Time");

            EventBus.EventBus.Instance.Subscribe<AutoColorGradingCompletedEvent>(a =>
            {
                var outputPath = _appSettings.MeasureResultOutputOption.OutputPath;
                var id = a.Report.ColorimetryReport.SerialNumber;
                var grade = a.Report.ColorGrade.Values;
                var lch = a.Report.ColorimetryReport.LCH;
                var xyz = a.Report.ColorimetryReport.XYZ;
                var lab = a.Report.ColorimetryReport.LAB;
                var whitePoint = a.Report.ColorimetryReport.Whitepoint;
                var now = a.Report.CreateDateTime;
                var temp1 = a.Report.ColorimetryReport.MeasurementTemperature;
                var message = $"{id},{grade.value},{grade.sufix1},{grade.sufix2},D65,{lch.L:F2},{lch.C:F4},{lch.H:F2},{lab.A:F4},{lab.B:F4},{xyz.X:F0},{xyz.Y:F0},{xyz.Z:F0},White,{whitePoint.X:F0},{whitePoint.Y:F0},{whitePoint.Z:F0},{systemSerialNumber},{now}";
                _measureResultWriter.Write(message);

                if (option.ColorGradingResultOutput)
                {
                    try
                    {
                        if(!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }
                        _measureResultWriter.Write(outputPath, $"{id}.cmi", message);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, "Measure result output error.");
                    }
                }
                //if(option.ColorGradingPixelsOutput)
                //{
                //    var pixels = a.Report.ColorimetryReport.Pixels;
                //    _measureResultWriter.Write($"{id}_{systemSerialNumber}", pixels);
                //}
            });

            EventBus.EventBus.Instance.Subscribe<AutoColorMeasuringCompletedEvent>(a =>
            {
                var id = a.Report.SerialNumber;
                var lch = a.Report.LCH;
                var xyz = a.Report.XYZ;
                var lab = a.Report.LAB;
                var whitePoint = a.Report.Whitepoint;
                var now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                var temp1 = a.Report.MeasurementTemperature;

                var message = $"{id},-,-,-,D65,{lch.L:F2},{lch.C:F4},{lch.H:F2},{lab.A:F4},{lab.B:F4},{xyz.X:F0},{xyz.Y:F0},{xyz.Z:F0},White,{whitePoint.X:F0},{whitePoint.Y:F0},{whitePoint.Z:F0},{systemSerialNumber},{now}";
                _measureResultWriter.Write(message);
            });

            EventBus.EventBus.Instance.Subscribe<ScanWhitepointCompletedEvent>(a =>
            {
                var message = @$"ptfe,{a.Whitepoint.X:F2},{a.Whitepoint.Y:F2},{a.Whitepoint.Z:F2},{a.WhitepointCorner.X:F2},{a.WhitepointCorner.Y:F2},{a.WhitepointCorner.Z:F2},{a.WhitepointStddev.X:F2},{a.WhitepointStddev.Y:F2},{a.WhitepointStddev.Z:F2},{a.Temperatue},{a.ProcessingDatetime}";
                _whitepointWriter.Write(message); 
            });

            EventBus.EventBus.Instance.Subscribe<ScanWhitepointOnSpectralonCompletedEvent>(a =>
            {
                var message = @$"spectralon,{a.Whitepoint.X:F2},{a.Whitepoint.Y:F2},{a.Whitepoint.Z:F2},{a.WhitepointCorner.X:F2},{a.WhitepointCorner.Y:F2},{a.WhitepointCorner.Z:F2},{a.WhitepointStddev.X:F2},{a.WhitepointStddev.Y:F2},{a.WhitepointStddev.Z:F2},{a.Temperatue},{a.ProcessingDatetime}";
                _whitepointWriter.Write(message);
            });
        }
    }
    public interface ILogger
    {
        void Infomation(string message);
        void Debug(string message);
        void Warning(string message);
        void Fatal(string message);
        void Error(Exception ex, string message);
    }
    public interface IErrorLogger
    {
        void Error(Exception ex, string message);
    }
    //public interface IPixelsFileStore<T>
    //{
    //    void Execute(string path, Pixels<T> obj); 
    //}
    //public interface IShadingPixelsFileLoader
    //{
    //    ShadingCorrectPixels Execte(string path);
    //}
    public interface IColorGradingConditonFileLoader
    {
        ColorGradingCondition Execute(string path);
    }
    //public interface IShadingPixelsFileStore
    //{
    //    void Execte(string path, ShadingCorrectPixels obj);
    //}
    public interface IPixelsTiffFileStore
    {
        void Execute(string path, XYZPixels obj);
        void Execute(string path, ShadingCorrectPixels obj);
        Task ExecuteAsync(string path, XYZPixels obj);
        Task ExecuteAsync(string path, ShadingCorrectPixels obj);
    }
    
    public interface IPixelsTiffFileLoader
    {
        ShadingCorrectPixels Execte(string path);
        Task<ShadingCorrectPixels> ExecuteAsync(string path);
    }

    public interface IInstrumentalErrorCorrectionMatrixFileLoader
    {
        Dictionary<string, double[]> Execute(string path);
    }
}
