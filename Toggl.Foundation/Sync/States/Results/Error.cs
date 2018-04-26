using System;

namespace Toggl.Foundation.Sync.States.Results
{
    public sealed class Error : IResult
    {
        public Exception Exception { get; }

        public IState NextState => null;

        public Error(Exception error)
        {
            Exception = error;
        }
    }
}
