using Toggl.Multivac;

namespace Toggl.Foundation.Sync.States.Results
{
    public sealed class Success : IResult
    {
        public IState NextState { get; }

        public Success(IState nextState)
        {
            Ensure.Argument.IsNotNull(nextState, nameof(nextState));

            NextState = nextState;
        }
    }
}
