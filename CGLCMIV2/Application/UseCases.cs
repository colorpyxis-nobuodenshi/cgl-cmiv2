using CGLCMIV2.Domain;
using CGLCMIV2.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CGLCMIV2.Application
{
    public interface IUsecase { }

    public class ConnectHardware : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        ILogger _logger;
        public ConnectHardware(ICamera camera, IAutoStage autoStage, ILEDLight led, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _logger = logger;
        }
        public void Execute()
        {
            try
            {
                EventBus.EventBus.Instance.Publish(new AppHardwareConnectingEvent());

                _camera.Start();
                _ledLight.Start();
                _autoStage.Start();
                
                _autoStage.MoveMechanicalHome();
                _autoStage.MoveHome();

                _logger.Infomation("hardware connected.");
                EventBus.EventBus.Instance.Publish(new AppHardwareConnectedEvent());
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "connect hardware.");
                EventBus.EventBus.Instance.Publish(new AppHardwareConnectedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }

    public class DisconnectHardware : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        ILogger _logger;
        public DisconnectHardware(ICamera camera, IAutoStage autoStage, ILEDLight led, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _logger = logger;
        }
        public void Execute()
        {
            try
            {
                EventBus.EventBus.Instance.Publish(new AppHardwareDisConnectingEvent());

                _camera.Stop();
                _autoStage.Stop();
                _ledLight.Stop();

                _logger.Infomation("hardware disconnected.");
                EventBus.EventBus.Instance.Publish(new AppHardwareDisConnectedEvent());
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "disconnect hardware.");
                EventBus.EventBus.Instance.Publish(new AppHardwareDisConnectedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }

    public class Start : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        ILogger _logger;
        IPixelsTiffFileStore _pixelsFileStore;
        double _opticalpowerFactor;
        int _whiteStageReplacementThreshold;
        public Start(ICamera camera, IAutoStage autoStage, ILEDLight led, IColorimetry colorimetry, ILogger logger, IPixelsTiffFileStore pixelsFileStore, AppSettings settings)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _colorimetry = colorimetry;
            _logger = logger;
            _pixelsFileStore = pixelsFileStore;
            _opticalpowerFactor = settings.MeasureCondition.OpticalpowerFactor;
            _whiteStageReplacementThreshold = settings.MeasureCondition.WhiteStageReplacementThreshold;
        }
        public void Execute(ColorimetryCondition condition)
        {
            try
            {
                //void ScanLEDPower()
                //{
                //    _ledLight.ChangeD65Value(1023);
                //    _ledLight.ChangeUVValue(0);
                //    var res1 = _ledLight.GetOpticalPower();
                //    _ledLight.ChangeD65Value(0);
                //    _ledLight.ChangeUVValue(1023);
                //    var res2 = _ledLight.GetOpticalPower();
                //    _ledLight.ChangeD65Value(1023);
                //    _ledLight.ChangeUVValue(32);

                //    EventBus.EventBus.Instance.Publish(new ScanLEDCompletedEvent(res1, res2));
                //}

                void ScanWhitepoint()
                {
                    _autoStage.MoveMeasurePoint();

                    var res = _colorimetry.ScanWhitepoint(condition);

                    var replacementTiming = Colorimetry.WhitepointComparisonWithSpectralon(res.whitepoint, res.whitepointCorner, condition.WhitepointOnSpectralon, condition.WhitepointForCorrectionOnSpectralon, _whiteStageReplacementThreshold);
                    //_ledLight.ReadStatus();
                    var temperature = _ledLight.GetTemperature();
                    var opticalpower = _ledLight.GetOpticalPower() * _opticalpowerFactor;

                    _logger.Infomation($"scan white point:{res.whitepoint}.");
                    if(!Directory.Exists("WhiteStage"))
                    {
                        Directory.CreateDirectory("WhiteStage");
                    }
                    var name = $"WhiteStage\\whitepoint{DateTime.Now.ToString("yyyyMMdd")}.tiff";
                    _pixelsFileStore.Execute(name, res.pixels);
                    EventBus.EventBus.Instance.Publish(new ScanWhitepointCompletedEvent(res.whitepoint, res.whitepointCorner, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), temperature, opticalpower, replacementTiming.replacement, res.whitepointStdDev));

                    _autoStage.MoveWorkSetPoint();
                }

                EventBus.EventBus.Instance.Publish(new AppStartingEvent());

                ScanWhitepoint();

                _logger.Infomation("started.");
                EventBus.EventBus.Instance.Publish(new AppStartedEvent());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "start.");
                EventBus.EventBus.Instance.Publish(new AppStartedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync(ColorimetryCondition condition)
        {
            await Task.Run(() => 
            {
                Execute(condition);
            });
        }
    }

    public class Stop : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        ILogger _logger;
        public Stop(ICamera camera, IAutoStage autoStage, ILEDLight led, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _logger = logger;
        }
        public void Execute()
        {
            try
            {
                EventBus.EventBus.Instance.Publish(new AppStoppingEvent());
                _autoStage.MoveWorkSetPoint();
                _logger.Infomation("stopped.");
                EventBus.EventBus.Instance.Publish(new AppStoppedEvent());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "stop.");
                EventBus.EventBus.Instance.Publish(new AppStoppedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => 
            {
                Execute();
            });
        }
    }

    public class Reset : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        ILogger _logger;
        public Reset(ICamera camera, IAutoStage autoStage, ILEDLight led, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _logger = logger;
        }
        public void Execute()
        {
            try
            {
                EventBus.EventBus.Instance.Publish(new AppResetingEvent());

                _ledLight.Stop();
                _autoStage.Stop();
                _camera.Stop();
                //_autoStage.MoveHome();

                _logger.Infomation("reseted.");
                EventBus.EventBus.Instance.Publish(new AppResetedEvent());

                EventBus.EventBus.Instance.Publish(new AppHardwareDisConnectedEvent());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "reset.");
                EventBus.EventBus.Instance.Publish(new AppResettedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => 
            {
                Execute();
            });
        }
    }
    public class Abort : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        public Abort(ICamera camera, IAutoStage autoStage, ILEDLight led)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
        }
        public void Execute()
        {
            try
            {
                EventBus.EventBus.Instance.Publish(new AppAbortingEvent());

                EventBus.EventBus.Instance.Publish(new AppAbortedEvent());

                //_autoStage.MoveHome();

                EventBus.EventBus.Instance.Publish(new AppStartedEvent());
            }
            catch (Exception ex)
            {
                EventBus.EventBus.Instance.Publish(new AppAbortedFailureEvent(ex));
            }
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    //public class ChangeModeAutoGrading : IUsecase
    //{
    //    public ChangeModeAutoGrading()
    //    {

    //    }
    //    public void Execute()
    //    {
    //        EventBus.EventBus.Instance.Publish(new AppModeChangedEvent(AppMode.AutoGrading));
    //    }
    //}
    //public class ChangeModeColorMeasure : IUsecase
    //{
    //    public ChangeModeColorMeasure()
    //    {

    //    }
    //    public void Execute()
    //    {
    //        EventBus.EventBus.Instance.Publish(new AppModeChangedEvent(AppMode.ColorMeasuring));
    //    }
    //}
    //public class ChangeModeDeviceConfiguration : IUsecase
    //{
    //    public ChangeModeDeviceConfiguration()
    //    {

    //    }
    //    public void Execute()
    //    {
    //        EventBus.EventBus.Instance.Publish(new AppModeChangedEvent(AppMode.Config));
    //    }
    //}
    //public class ChangeModeNone : IUsecase
    //{
    //    public ChangeModeNone()
    //    {

    //    }
    //    public void Execute()
    //    {
    //        EventBus.EventBus.Instance.Publish(new AppModeChangedEvent(AppMode.None));
    //    }
    //}

    //public class StartAutoGrading : IUsecase
    //{
    //    public StartAutoGrading()
    //    {

    //    }
    //    public void Execute()
    //    {

    //    }
    //}
    //public class StopAutoGrading : IUsecase
    //{
    //    public StopAutoGrading()
    //    {

    //    }
    //    public void Execute()
    //    {

    //    }
    //}

    //public class StartColorMeasure : IUsecase
    //{
    //    public StartColorMeasure()
    //    {

    //    }
    //    public void Execute()
    //    {

    //    }
    //}

    public class CameraCaptureSnap : IUsecase
    {
        ICamera _camera;
        public CameraCaptureSnap(ICamera camera)
        {
            _camera = camera;
        }
        public void Execute()
        {
            throw new NotImplementedException();
        }

        public async Task<XYZPixels> ExecuteAsync(CancellationTokenSource stopToken, int exposureTime, int integration = 1)
        {
            return await Task.Run(() =>
            {

                var p = _camera.TakePicture(exposureTime, integration);
                return p;
            }, stopToken.Token);
        }
    }

    public class CameraCaptureStart : IUsecase
    {
        ICamera _camera;
        public CameraCaptureStart(ICamera camera)
        {
            _camera = camera;
        }
        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Execute(Action<XYZPixels> callback, CancellationTokenSource stopToken, int exposureTime = 1, int integration = 1, ShadingCorrectPixels shd = null, MultiColorConversionMatrix ccm = null)
        {

            Task.Run(() =>
            {
                while (!stopToken.IsCancellationRequested)
                {
                    var p = _camera.TakePicture(exposureTime, integration);
                    if(shd is not null)
                    {
                        p = Colorimetry.CorrectShading(p, shd);
                    }
                    if(ccm is not null)
                    {
                        p = Colorimetry.ConvertRAW2XYZ(p, ccm);
                    }
                    callback(p);
                    Task.Delay(100);
                }
            }, stopToken.Token);

        }

        public async Task ExecuteAsync(Action<XYZPixels> callback, CancellationTokenSource stopToken, int exposureTime, int integration = 1)
        {

            await Task.Run(async () =>
            {
                while (!stopToken.IsCancellationRequested)
                {
                    var p = _camera.TakePicture(exposureTime, integration);
                    callback(p);
                    await Task.Delay(200);
                }
            }, stopToken.Token);
        }
    }

    public class CameraCaptureStop : IUsecase
    {
        public CameraCaptureStop()
        {
        }
        public void Execute()
        {
            throw new NotImplementedException();
        }
        public void Execute(CancellationTokenSource stopToken)
        {
            stopToken?.Cancel(true);

        }
        public async Task ExecuteAsync(CancellationTokenSource stopToken)
        {
            stopToken?.Cancel(true);
            
            await Task.CompletedTask;
            
        }
    }

    public class LEDPowerChange : IUsecase
    {
        ILEDLight _ledLight;
        public LEDPowerChange(ILEDLight led)
        {
            _ledLight = led;
        }

        public void Execute()
        {
            
        }

        public void Execute(int d65, int uv)
        {
            _ledLight.ChangeD65Value(d65);
            _ledLight.ChangeUVValue(uv);
            _ledLight.StoreValues(d65, uv);

            EventBus.EventBus.Instance.Publish(new ChangeLEDPowerCompletedEvent(d65, uv, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
        }

        public async Task ExecuteAsync(int d65, int uv)
        {
            await Task.Run(() =>
            {
                Execute(d65, uv);
            });
        }
    }
    //public class LEDOpticalPowerCheck : IUsecase
    //{
    //    ILEDLight _ledLight;
    //    public LEDOpticalPowerCheck(ILEDLight led)
    //    {
    //        _ledLight = led;
    //    }

    //    public void Execute()
    //    {
            
    //    }

    //    public void Execute(Action<object> callback)
    //    {
    //        var res = _ledLight.GetOpticalPower();
    //        callback(res);
    //    }
    //}

    public class AutoStageMoveORG : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveORG(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveMechanicalHome();
        }

        public async Task ExecuteAsync()
        {
            await Task.Run(() => 
            {
                Execute();
            });
        }
    }
    public class AutoStageMoveHome : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveHome(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveHome();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageMoveWorkSetPoint : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveWorkSetPoint(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveWorkSetPoint();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }

    public class AutoStageMoveMeasurePoint : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveMeasurePoint(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveMeasurePoint();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageMoveMeasurePointOnSpectralon : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveMeasurePointOnSpectralon(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveMeasurePointOnSpectralon();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageMoveReplacementPoint : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageMoveReplacementPoint(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.MoveReplacementPoint();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageRotateCW45 : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageRotateCW45(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.RotateCW45();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageRotateCCW45 : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageRotateCCW45(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.RotateCCW45();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageRotateHome : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageRotateHome(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.Rotate(0);
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageRotateCWJ : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageRotateCWJ(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.RotateCWJ();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoStageRotateCCWJ : IUsecase
    {
        IAutoStage _autoStage;
        public AutoStageRotateCCWJ(IAutoStage stage)
        {
            _autoStage = stage;
        }

        public void Execute()
        {
            _autoStage.RotateCCWJ();
        }
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                Execute();
            });
        }
    }
    public class AutoGradingStart : IUsecase
    {
        
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        IColorGradeJudgement _colorGradeJudgement;
        ILogger _logger;
        public AutoGradingStart(ICamera camera, IAutoStage autoStage, ILEDLight led, IColorimetry colorimetry, IColorGradeJudgement colorGradeJudgement, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _colorimetry = colorimetry;
            _colorGradeJudgement = colorGradeJudgement;
            _logger = logger;
        }

        enum Status
        {
            Started,
            MovedStageMeasurePoint,
            MeasuredColor,
            MeasuredColorCompleted,
            MovedStageRotate45,
            MovedStageWorkSetPoint,
            Analized,
            Completed,
            Error,
        }
        public async Task ExecuteAsync(string serialNumber, ColorimetryCondition condition, ColorGradingCondition condition2, CancellationTokenSource cancellationTokenSource)
        {
            void MoveMeasurePoint()
            {
                try
                {
                    _autoStage.MoveMeasurePoint();
                }
                catch(Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "color grading.");
                }
            }

            void MoveWorkSetPoint()
            {
                try
                {
                    _autoStage.MoveWorkSetPoint();
                }
                catch (Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "color grading.");
                }
            }

            void MoveRotate()
            {
                try
                { 
                    _autoStage.RotateCW45();
                }
                catch (Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "color grading.");
                }
            }
            
            ColorimetryResult Measure(ColorimetryCondition condition)
            {
                return _colorimetry.Measure(condition);
            }
            ColorGradeReport Analize(ColorimetryResult[] result, string serialNumber)
            {
                var r = ColorimetryReport.Aggregate(result, serialNumber);
                var grade = _colorGradeJudgement.Execute(r.LCH, condition2);
                return new ColorGradeReport(grade, r); 
            }
            ColorGradeReport report = null;
            var status = Status.Started;
            var results = new List<ColorimetryResult>();
            var measureCount = 0;
            var token = cancellationTokenSource.Token;

            await Task.Run(() => 
            {
                try
                {
                    
                    while (status != Status.Completed)
                    {
                        switch (status)
                        {
                            case Status.Started:
                                _logger.Infomation("color grading started.");
                                MoveMeasurePoint();
                                status = Status.MovedStageMeasurePoint;
                                break;
                            case Status.MovedStageMeasurePoint:
                            case Status.MovedStageRotate45:
                                var res = Measure(condition);
                                EventBus.EventBus.Instance.Publish(new AutoColorGradingColorimetryResultChangedEvent(res));
                                results.Add(res);
                                measureCount++;
                                //_logger.Infomation($"measure result : L*C*h*={res.CenterOfGravityLCH}, L*a*b*={res.CenterOfGravityLAB}, XYZ={res.CenterOfGravityXYZ}, Whitepoint={res.Whitepoint}, Whitepoint Reference={condition.Whitepoint}.");
                                if (measureCount == 8)
                                    status = Status.MeasuredColorCompleted;
                                else
                                    status = Status.MeasuredColor;
                                break;
                            case Status.MeasuredColor:
                                MoveRotate();
                                status = Status.MovedStageRotate45;
                                break;
                            case Status.MeasuredColorCompleted:
                                //MoveRotate();
                                MoveWorkSetPoint();
                                report = Analize(results.ToArray(), serialNumber);
                                status = Status.Analized;
                                break;
                            case Status.Analized:
                                EventBus.EventBus.Instance.Publish(new AutoColorGradingCompletedEvent(report, condition));
                                status = Status.Completed;
                                _logger.Infomation("color grading completed.");
                                _logger.Infomation($"measure result : grade={report.ColorGrade},L*C*h*={report.ColorimetryReport.LCH}, L*a*b*={report.ColorimetryReport.LAB}, XYZ={report.ColorimetryReport.XYZ}, Whitepoint={report.ColorimetryReport.Whitepoint}.");
                                break;
                        }
                        if (token.IsCancellationRequested)
                        {
                            status = Status.Completed;
                            token.ThrowIfCancellationRequested();
                        }
                    }
                    
                }
                catch(OperationCanceledException ex)
                {
                    MoveWorkSetPoint();
                    _logger.Warning("color grading cancel.");
                    EventBus.EventBus.Instance.Publish(new AutoGradingCancelEvent());
                }
                catch (Exception ex)
                {
                    MoveWorkSetPoint();
                    _logger.Error(ex, "color grading.");
                    EventBus.EventBus.Instance.Publish(new AutoGradingFailureEvent(ex));
                }
            }, token);
        }
    }

    public class ColorMeasuringStart : IUsecase
    {
        ICamera _camera;
        IColorimetry _colorimetry;
        public ColorMeasuringStart(ICamera camera, IColorimetry colorimetry)
        {
            _camera = camera;
            _colorimetry = colorimetry;
        }

        public void Execute(string serialNumber, ColorimetryCondition condition)
        {
            var r1 = _colorimetry.Measure(condition);
            var r2 = ColorimetryReport.Create(r1, serialNumber);
            EventBus.EventBus.Instance.Publish(new AutoColorMeasuringCompletedEvent(r2, condition));
            EventBus.EventBus.Instance.Publish(new AutoColorMeasuringColorimetryResultChangedEvent(r1));
        }
        public async Task ExecuteAsync(string serialNumber, ColorimetryCondition condition)
        {
            await Task.Run(() =>
            {
                Execute(serialNumber, condition);
            });
        }
    }
    public class AutoColorMeasuringStart : IUsecase
    {

        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        ILogger _logger;
        public AutoColorMeasuringStart(ICamera camera, IAutoStage autoStage, ILEDLight led, IColorimetry colorimetry, ILogger logger)
        {
            _camera = camera;
            _autoStage = autoStage;
            _ledLight = led;
            _colorimetry = colorimetry;
            _logger = logger;
        }

        
        enum Status
        {
            Started,
            MovedStageMeasurePoint,
            MeasuredColor,
            MeasuredColorCompleted,
            MovedStageRotate45,
            MovedStageWorkSetPoint,
            Analized,
            Completed,
            Error,
        }
        public async Task ExecuteAsync(string serialNumber, ColorimetryCondition condition, CancellationTokenSource cancellationTokenSource)
        {
            void MoveMeasurePoint()
            {
                try
                {
                    _autoStage.MoveMeasurePoint();
                }
                catch(Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "auto color measuring.");
                }
            }

            void MoveWorkSetPoint()
            {
                try
                { 
                    _autoStage.MoveWorkSetPoint();
                }
                catch (Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "auto color measuring.");
                }
            }

            void MoveRotate()
            {
                try
                { 
                    _autoStage.RotateCW45();
                }
                catch (Exception ex)
                {
                    _autoStage.Stop();
                    _autoStage.Start();
                    _logger.Error(ex, "auto color measuring.");
                }
            }
            ColorimetryResult Measure(ColorimetryCondition condition)
            {
                return _colorimetry.Measure(condition);
            }
            ColorimetryReport Analize(ColorimetryResult[] result, string serialNumber)
            {
                var r = ColorimetryReport.Aggregate(result, serialNumber);
                return r;
            }
            ColorimetryReport report = null;
            var status = Status.Started;
            var results = new List<ColorimetryResult>();
            var measureCount = 0;
            var token = cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                try
                {
                    while (status != Status.Completed)
                    {
                        switch (status)
                        {
                            case Status.Started:
                                _logger.Infomation("auto color measuring started.");
                                MoveMeasurePoint();
                                status = Status.MovedStageMeasurePoint;
                                break;
                            case Status.MovedStageMeasurePoint:
                            case Status.MovedStageRotate45:
                                var res = Measure(condition);
                                EventBus.EventBus.Instance.Publish(new AutoColorMeasuringColorimetryResultChangedEvent(res));
                                results.Add(res);
                                measureCount++;
                                if (measureCount == 8)
                                    status = Status.MeasuredColorCompleted;
                                else
                                    status = Status.MeasuredColor;
                                break;
                            case Status.MeasuredColor:
                                MoveRotate();
                                status = Status.MovedStageRotate45;
                                break;
                            case Status.MeasuredColorCompleted:
                                //MoveRotate();
                                MoveWorkSetPoint();
                                report = Analize(results.ToArray(), serialNumber);
                                status = Status.Analized;
                                break;
                            case Status.Analized:
                                EventBus.EventBus.Instance.Publish(new AutoColorMeasuringCompletedEvent(report, condition));
                                status = Status.Completed;
                                _logger.Infomation("auto color measuring completed.");
                                _logger.Infomation($"measure result : L*C*h*={report.LCH}, XYZ={report.XYZ}, Whitepoint={report.Whitepoint}.");
                                break;
                        }
                        if (token.IsCancellationRequested)
                        {
                            MoveWorkSetPoint();
                            status = Status.Completed;
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch(OperationCanceledException ex)
                {
                    _logger.Warning("auto color measuring cancel.");
                    EventBus.EventBus.Instance.Publish(new AutoColorMeasureCancelEvent());
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "auto color measuring.");
                    EventBus.EventBus.Instance.Publish(new AutoColorMeasureFailureEvent(ex));
                }
            }, token);
        }
    }

    public class ScanWhitepoint : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        IPixelsTiffFileStore _pixelsFileStore;
        ILogger _logger;
        double _opticalpowerFactor;
        int _whiteStageReplacementThreshold;
        public ScanWhitepoint(ICamera camera, IColorimetry colorimetry, IAutoStage autoStage, ILEDLight led, IPixelsTiffFileStore pixelsFileStore, ILogger logger, AppSettings settings)
        {
            _camera = camera;
            _colorimetry = colorimetry;
            _autoStage = autoStage;
            _ledLight = led;
            _pixelsFileStore = pixelsFileStore;
            _logger = logger;
            _opticalpowerFactor = settings.MeasureCondition.OpticalpowerFactor;
            _whiteStageReplacementThreshold = settings.MeasureCondition.WhiteStageReplacementThreshold;
        }

        public void Execute(ColorimetryCondition condition)
        {
            try
            {
                _autoStage.MoveMeasurePoint();
                var res = _colorimetry.ScanWhitepoint(condition);
                var replacementTiming = Colorimetry.WhitepointComparisonWithSpectralon(res.whitepoint, res.whitepointCorner, condition.WhitepointOnSpectralon, condition.WhitepointForCorrectionOnSpectralon, _whiteStageReplacementThreshold);
                //_ledLight.ReadStatus();
                var temperature = _ledLight.GetTemperature();
                var opticalpower = _ledLight.GetOpticalPower() * _opticalpowerFactor;

                _autoStage.MoveWorkSetPoint();

                var name = $"WhiteStage\\whitepoint{DateTime.Now.ToString("yyyyMMdd")}.tiff";
                _pixelsFileStore.Execute(name, res.pixels);

                EventBus.EventBus.Instance.Publish(new ScanWhitepointCompletedEvent(res.whitepoint, res.whitepointCorner, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), temperature, opticalpower, replacementTiming.replacement, res.whitepointStdDev));
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "scan whitepoint.");
                throw;
            }
        }

        public async Task ExecuteAsync(ColorimetryCondition condition)
        {
            await Task.Run(() => 
            {
                Execute(condition);
            });
        }
    }


    public class ScanWhitepointOnSpectralon : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        IPixelsTiffFileStore _pixelsFileStore;
        ILogger _logger;
        double _opticalpowerFactor;
        public ScanWhitepointOnSpectralon(ICamera camera, IColorimetry colorimetry, IAutoStage autoStage, ILEDLight led, IPixelsTiffFileStore pixelsFileStore, ILogger logger, AppSettings settings)
        {
            _camera = camera;
            _colorimetry = colorimetry;
            _autoStage = autoStage;
            _ledLight = led;
            _pixelsFileStore = pixelsFileStore;
            _logger = logger;
            _opticalpowerFactor = settings.MeasureCondition.OpticalpowerFactor;
        }

        public void Execute(ColorimetryCondition condition)
        {
            try
            {
                _autoStage.MoveMeasurePointOnSpectralon();
                var res = _colorimetry.ScanWhitepoint(condition);
                //_ledLight.ReadStatus();
                var temperature = _ledLight.GetTemperature();
                var opticalpower = _ledLight.GetOpticalPower() * _opticalpowerFactor;

                _autoStage.MoveReplacementPoint();

                var name = $"WhiteStage\\spectralon{DateTime.Now.ToString("yyyyMMdd")}.tiff";
                _pixelsFileStore.Execute(name, res.pixels);

                EventBus.EventBus.Instance.Publish(new ScanWhitepointOnSpectralonCompletedEvent(res.whitepoint, res.whitepointCorner, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), temperature, opticalpower, res.whitepointStdDev));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "scan whitepoint on spectralon.");
                throw;
            }
        }

        public async Task ExecuteAsync(ColorimetryCondition condition)
        {
            await Task.Run(() =>
            {
                Execute(condition);
            });
        }
    }
    //public class ScanLED : IUsecase
    //{
    //    ILEDLight _ledLight;
    //    public ScanLED(ILEDLight led)
    //    {
    //        _ledLight = led;
    //    }

    //    public void Execute()
    //    {
    //        _ledLight.ChangeD65Value(1023);
    //        _ledLight.ChangeUVValue(0);
    //        var res1 = _ledLight.GetOpticalPower();
    //        _ledLight.ChangeD65Value(0);
    //        _ledLight.ChangeUVValue(1023);
    //        var res2 = _ledLight.GetOpticalPower();
    //        _ledLight.ChangeD65Value(1023);
    //        _ledLight.ChangeUVValue(32);

    //        EventBus.EventBus.Instance.Publish(new ScanLEDCompletedEvent(res1, res2));
    //    }

    //    public async Task ExecuteAsync(ColorimetryCondition condition)
    //    {
    //        await Task.Run(() =>
    //        {
    //            Execute();
    //        });
    //    }
    //}

    public class MakeShadingData : IUsecase
    {
        ICamera _camera;
        IAutoStage _autoStage;
        ILEDLight _ledLight;
        IColorimetry _colorimetry;
        IPixelsTiffFileStore _store;
        ILogger _logger;
        public MakeShadingData(ICamera camera, IAutoStage autoStage, ILEDLight led, IColorimetry colorimetry, IPixelsTiffFileStore store, ILogger logger)
        {
            _camera = camera;
            _colorimetry = colorimetry;
            _autoStage = autoStage;
            _ledLight = led;
            _store = store;
            _logger = logger;
        }

        public void Execute(ColorimetryCondition condition)
        {
            try
            {
                _autoStage.MoveMeasurePoint();

                //var shd = _colorimetry.MakeShadingData(condition);
                var p = _camera.TakePicture(condition.ExposureTime, condition.Integration);
                var shd = Colorimetry.MakeShadingData(p);

                _autoStage.MoveWorkSetPoint();

                _store.Execute("shd.tiff", shd);

                EventBus.EventBus.Instance.Publish(new MakeShadingCorrectDataCompletedEvent(shd, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "make shading data.");
                throw;
            }
        }

        public async Task ExecuteAsync(ColorimetryCondition condition)
        {
            await Task.Run(() =>
            {
                Execute(condition);
            });
        }
    }

    public class BarcodeRead : IUsecase
    {
        public BarcodeRead()
        {
            
        }

        public void Execute(string value)
        {
            EventBus.EventBus.Instance.Publish(new BarcodeReadEvnet1(value));
        }

        public async Task ExecuteAsync(string value)
        {
            await Task.Run(() =>
            {
                Execute(value);
            });
        }
    }
}
