using NSubstitute;
using Toggl.PrimeRadiant.Realm;

namespace Toggl.PrimeRadiant.Tests.Realm
{
    public sealed class RealmRepositoryTests : RepositoryTests<TestModel>
    {
        protected override IRepository<TestModel> Repository { get; } = new Repository<TestModel>(
            new TestAdapter(),
            (a, b) => ConflictResolutionMode.Ignore,
            Substitute.For<IRivalsResolver<TestModel>>());

        protected override TestModel GetModelWith(int id) => new TestModel { Id = id };
    }
}
