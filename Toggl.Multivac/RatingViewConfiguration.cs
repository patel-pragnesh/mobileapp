namespace Toggl.Multivac
{
    public struct RatingViewConfiguration
    {
        public int DayCount { get; }

        public RatingViewCriterion Criterion { get; }

        public RatingViewConfiguration(int dayCount, RatingViewCriterion criterion)
        {
            DayCount = dayCount;
            Criterion = criterion;
        }
    }

    public enum RatingViewCriterion
    {
        None,
        Stop,
        Start,
        Continue
    }
}
