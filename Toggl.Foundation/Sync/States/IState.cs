using System;
using System.Collections.Generic;
using Toggl.Foundation.Sync.States.Results;

namespace Toggl.Foundation.Sync.States
{
    public interface IState
    {
        IEnumerable<IResult> AllPossibleOutcomes { get; }

        IObservable<IResult> Run();
    }
}
