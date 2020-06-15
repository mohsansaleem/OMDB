using System;
using Zenject;
using UniRx;

namespace OMDB.Generic
{
    public abstract class Mediator : IInitializable, IDisposable
    {
        [Inject] protected readonly SignalBus SignalBus;

        protected readonly CompositeDisposable Disposables;

        protected Mediator()
        {
            Disposables = new CompositeDisposable();
        }

        public virtual void Initialize()
        {
            // Initialize stuff here.
        }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}

