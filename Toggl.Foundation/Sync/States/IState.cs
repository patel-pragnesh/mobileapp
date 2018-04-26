using System;
using System.Collections.Generic;
using System.Reactive;

namespace Toggl.Foundation.Sync.States
{
    public interface IState
    {
        IEnumerable<IState> AllPossibleOutcomes { get; }

        IObservable<IResult> Run(IObservable<Unit> abort);
    }
}
