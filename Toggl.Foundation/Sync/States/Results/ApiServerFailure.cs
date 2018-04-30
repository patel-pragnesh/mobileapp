using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Results
{
    internal sealed class ApiServerFailure : IResult
    {
        public ApiException Exception { get; }

        public ApiServerFailure(ApiException error = null)
        {
            Exception = error;
        }
    }
}
