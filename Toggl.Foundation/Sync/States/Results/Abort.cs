namespace Toggl.Foundation.Sync.States.Results
{
    public sealed class Abort : IResult
    {
        public IState NextState => null;
    }
}