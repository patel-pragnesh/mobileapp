using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Multivac;

namespace Toggl.Ultrawave.ApiClients
{
    public interface IFeedbackApiClient
    {
        IObservable<Unit> Send(Email email, string message, IDictionary<string, string> data);
    }
}
