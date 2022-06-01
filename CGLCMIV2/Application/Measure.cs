using CGLCMIV2.Domain;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CGLCMIV2.Application
{
    //public class ColorMeasure
    //{
    //    public enum Status
    //    {
    //        Ready,
    //        Starting,
    //        Started,
    //        Rotating,
    //        Rotated,
    //        Measuring,
    //        Measured,
    //        Stopping,
    //        Stopped,
    //        Error
    //    }
    //    public enum Trigger
    //    {
    //        Start,
    //        Stop,
    //        Measure,
    //        Rotate,
    //        Reset,
    //        Error
    //    }

    //    public ColorMeasure(ICamera _camera, IAutoStage _autoStage, ILEDLight _ledLight, ColorimetryCondition _condition)
    //    {

    //    }
    //    Status _currentState = Status.Ready;
    //    BehaviorSubject<Status> _subject;
    //    ICamera _camera;
    //    IAutoStage _autoStage;
    //    ILEDLight _ledLight;
    //    ColorimetryCondition _condition;
    //    void Do(Trigger trigger)
    //    {
    //        switch(_currentState)
    //        {
    //            case Status.Ready:
    //                if(trigger == Trigger.Start)
    //                {
    //                    Start();
    //                    _currentState = Status.Started;
    //                }
    //                break;
    //            case Status.Started:
    //            case Status.Rotated:
    //            case Status.Measured:
    //                if (trigger == Trigger.Rotate)
    //                {
    //                    Rotate();
    //                    _currentState = Status.Rotated;
    //                }
    //                if (trigger == Trigger.Measure)
    //                {
    //                    Measure();
    //                    _currentState = Status.Measured;
    //                }
    //                if(trigger == Trigger.Stop)
    //                {
    //                    Stop();
    //                    _currentState = Status.Stopped;
    //                }
    //                break;
    //            case Status.Stopped:
    //                _currentState = Status.Ready;
    //                break;
    //            default:
    //                if(trigger == Trigger.Error)
    //                    _currentState = Status.Error;
    //                break;

    //        }
    //        _subject.OnNext(_currentState);
    //    }
    //    public void Dispath(Trigger trigger)
    //    {
    //        Do(trigger);
    //    }

    //    public void Start()
    //    {
    //        _autoStage.MoveMeasurePoint();
    //    }
    //    public void Stop()
    //    {
    //        _autoStage.MoveWorkSetPoint();
    //    }

    //    public void Rotate()
    //    {
    //        _autoStage.Rotate45();
    //    }
    //    public void Measure()
    //    {
    //        var c = new Colorimetry(_camera);
    //        //var res = c.Measure(_condition);
            
    //    }
    //    public void Reset()
    //    {

    //    }
    //    public IDisposable Subscribe(Action<Status> action)
    //    {

    //        return _subject
    //            .AsObservable()
    //            .Subscribe(action);
    //    }
    //}
}
