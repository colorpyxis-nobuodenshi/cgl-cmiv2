using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace EventBus
{
    public interface IEvent
    {

    }
    public class EventBus : IDisposable
    {
        readonly BehaviorSubject<IEvent> _subject = new (default(IEvent));

        public static EventBus Instance { get; } = new EventBus();

        public IDisposable Subscribe<T>(Action<T> action) where T : IEvent
        {

            return _subject.OfType<T>()
            .AsObservable()
            .ObserveOn(System.Threading.SynchronizationContext.Current)
            .Subscribe(action);
        }
        public void Publish<T>(T @event) where T : IEvent
        {
            _subject.OnNext(@event);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
