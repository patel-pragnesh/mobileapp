using Toggl.Multivac;

namespace Toggl.Foundation.Sync.States.Results
{
    public sealed class Proceed : IResult
    {
        public IState NextState { get; }

        public Proceed(IState nextState)
        {
            Ensure.Argument.IsNotNull(nextState, nameof(nextState));

            NextState = nextState;
        }
    }
}
