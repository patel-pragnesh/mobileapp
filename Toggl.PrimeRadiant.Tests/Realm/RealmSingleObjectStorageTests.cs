using Toggl.PrimeRadiant.Realm;

namespace Toggl.PrimeRadiant.Tests.Realm
{
    public sealed class RealmSingleObjectStorageTests : SingleObjectStorageTests<TestModel>
    {
        protected override ISingleObjectStorage<TestModel> Storage { get; } 
            = new SingleObjectStorage<TestModel>(new TestAdapter());

        protected override TestModel GetModelWith(int id) => new TestModel { Id = id };
    }
}
