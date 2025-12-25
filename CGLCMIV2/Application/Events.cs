using CGLCMIV2.Domain;
using EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Application
{
    public class AppLifecycleEvent : IEvent { }
    public class AppHardwareConnectingEvent : AppLifecycleEvent { }
    public class AppHardwareConnectedEvent : AppLifecycleEvent { }
    public class AppHardwareDisConnectingEvent : AppLifecycleEvent { }
    public class AppHardwareDisConnectedEvent : AppLifecycleEvent { }
    public class AppStartingEvent : AppLifecycleEvent { }
    public class AppStartedEvent : AppLifecycleEvent { }
    public class AppStoppingEvent : AppLifecycleEvent { }
    public class AppStoppedEvent : AppLifecycleEvent { }
    public class AppResetingEvent : AppLifecycleEvent { }
    public class AppResetedEvent : AppLifecycleEvent { }
    public class AppAbortingEvent : AppLifecycleEvent { }
    public class AppAbortedEvent : AppLifecycleEvent { }
    public class AppErrorEvent : AppLifecycleEvent 
    {
        public AppErrorEvent(Exception ex)
        {
            Exception = ex;
            Message = ex.Message;
        }
        public Exception Exception { get; }
        public string Message { get; }
    }
    public class AppHardwareConnectedFailureEvent : AppErrorEvent
    { 
        public AppHardwareConnectedFailureEvent(Exception ex) : base(ex) 
        { 
        }
    }
    public class AppHardwareDisConnectedFailureEvent : AppErrorEvent
    { 
        public AppHardwareDisConnectedFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AppStartedFailureEvent : AppErrorEvent
    {
        public AppStartedFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AppStoppedFailureEvent : AppErrorEvent
    {
        public AppStoppedFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AppResettedFailureEvent : AppErrorEvent
    {
        public AppResettedFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AppAbortedFailureEvent : AppErrorEvent
    {
        public AppAbortedFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AppWarnEvent : AppLifecycleEvent { }
    public class AppMode : IEvent { }
    public class AppModeHome : AppMode { }
    public class AppModeColorGrading : AppMode { }
    public class AppModeColorMeasuring : AppMode { }
    public class AppModeCalibration : AppMode { }
    public class AppModeConfiguration : AppMode { }

    //public class AppModeChangedEvent : IEvent
    //{
    //    public AppModeChangedEvent(AppMode mode)
    //    {
    //        Mode = mode;
    //    }
    //    public AppMode Mode { get; private set; }
    //}
    //public class AppStateChangedEvent : IEvent
    //{
    //    public AppStateChangedEvent(AppStatus status)
    //    {
    //        Status = status;
    //    }
    //    public AppStatus Status { get; private set; }
    //}
    public class AutoColorGradingCompletedEvent : IEvent
    {
        public AutoColorGradingCompletedEvent(ColorGradeReport report, ColorimetryCondition condition)
        {
            Report = report;
            Condition = condition;
        }

        public ColorGradeReport Report { get; }
        public ColorimetryCondition Condition { get; }
    }

    public class AutoColorMeasuringCompletedEvent : IEvent
    {
        public AutoColorMeasuringCompletedEvent(ColorimetryReport report, ColorimetryCondition condition)
        {
            Report = report;
            Condition = condition;
        }

        public ColorimetryReport Report { get; }
        public ColorimetryCondition Condition { get; }
    }
    public class ColorMeasureCompletedEvent : IEvent
    {
        public ColorMeasureCompletedEvent(ColorimetryReport report)
        {
            Report = report;
        }

        public ColorimetryReport Report { get; }
    }
    public class AutoGradingCancelEvent : IEvent { }
    public class AutoColorMeasureCancelEvent : IEvent { }
    public class AutoGradingFailureEvent : AppErrorEvent
    {
        public AutoGradingFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AutoColorMeasureFailureEvent : AppErrorEvent
    {
        public AutoColorMeasureFailureEvent(Exception ex) : base(ex)
        {
        }
    }
    public class AutoGradingStateChangedEvent : IEvent
    {
        public AutoGradingStateChangedEvent(AutoGradingStatus status)
        {
            Status = status;
        }
        public AutoGradingStatus Status { get; private set; }
    }

    public class ColorMeasuringStateChangedEvent : IEvent
    {
        public ColorMeasuringStateChangedEvent(ColorMeasuringStatus status)
        {
            Status = status;
        }
        public ColorMeasuringStatus Status { get; private set; }
    }
    public class ScanWhitepointCompletedEvent : IEvent
    {
        public ScanWhitepointCompletedEvent(CIEXYZ whitepoint, CIEXYZ whitepointCorner, string processingDatetime, double temperature, double opticalpower)
        {
            Whitepoint = whitepoint;
            WhitepointCorner = whitepointCorner;
            ProcessingDatetime = processingDatetime;
            Temperatue = temperature;
            Opticalpower = opticalpower;
        }
        public CIEXYZ Whitepoint { get; }
        public CIEXYZ WhitepointCorner { get; }
        public string ProcessingDatetime { get; }
        public double Temperatue { get; }
        public double Opticalpower { get; }
    }

    public class ScanLEDCompletedEvent : IEvent
    {
        public ScanLEDCompletedEvent(double p1, double p2)
        {
            D65Power = p1;
            UVPower = p2;
        }
        public double D65Power { get; private set; }
        public double UVPower { get; private set; }

    }

    public class ChangeLEDPowerCompletedEvent : IEvent
    {
        public ChangeLEDPowerCompletedEvent(int p1, int p2, string processingDate)
        {
            D65Power = p1;
            UVPower = p2;
            ProcessingDate = processingDate;
        }
        public int D65Power { get; }
        public int UVPower { get; }
        public string ProcessingDate { get; }
    }

    public class TemperatureChangedEvent : IEvent
    {
        public TemperatureChangedEvent(double temp)
        {
            Temperature = temp;
        }

        public double Temperature { get; private set; }
    }
    public class OpticalpowerChangedEvent : IEvent
    {
        public OpticalpowerChangedEvent(double value)
        {
            Opticalpower = value;
        }

        public double Opticalpower { get; private set; }
    }
    public class OperatingTimeCountEvent : IEvent
    {
        public OperatingTimeCountEvent(double count)
        {
            Count = count;
        }
        public double Count { get; private set; }
    }

    public class ColorimetryConditionChangedEvent : IEvent
    {
        public ColorimetryConditionChangedEvent(ColorimetryCondition condition)
        {
            ColorimetryCondition = condition;
        }
        public ColorimetryCondition ColorimetryCondition { get; }
    }

    //public class ColorMeasuringCompletedEvent : IEvent
    //{
    //    public ColorMeasuringCompletedEvent(ColorimetryReport report)
    //    {
    //        ColorimetryReport = report;
    //    }
    //    public ColorimetryReport ColorimetryReport { get; }
    //}

    public class ColorGradingCompletedEvent : IEvent
    {
        public ColorGradingCompletedEvent(ColorGradeReport report)
        {
            ColorGradeReport = report;
        }
        public ColorGradeReport ColorGradeReport { get; }
    }

    public class BarcodeReadEvnet1 : IEvent
    {
        public BarcodeReadEvnet1(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }

    public class BarcodeReadEvnet2 : IEvent
    {
        public BarcodeReadEvnet2(string value)
        {
            Value = value;
        }
        public string Value { get; }
    }

    public class MakeShadingCorrectDataCompletedEvent : IEvent
    {
        public MakeShadingCorrectDataCompletedEvent(ShadingCorrectPixels value, string processingDatetime)
        {
            ShadingCorrectPixels = value;
            ProcessingDatetime = processingDatetime;
        }

        public ShadingCorrectPixels ShadingCorrectPixels { get; }
        public string ProcessingDatetime { get; }
    }

    public class AutoColorMeasuringColorimetryResultChangedEvent : IEvent
    {
        public AutoColorMeasuringColorimetryResultChangedEvent(ColorimetryResult result)
        {
            Result = result;
        }
        public ColorimetryResult Result { get; }
    }

    public class AutoColorGradingColorimetryResultChangedEvent : IEvent
    {
        public AutoColorGradingColorimetryResultChangedEvent(ColorimetryResult result)
        {
            Result = result;
        }
        public ColorimetryResult Result { get; }
    }

    public class ChangeMeasureResultOutputPath : IEvent
    {
        public ChangeMeasureResultOutputPath(string path)
        {
            MeasureResultOutputPath = path;
        }

        public string MeasureResultOutputPath { get; }
    }
}
