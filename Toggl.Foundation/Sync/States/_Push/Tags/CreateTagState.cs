using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class CreateTagState : BaseCreateEntityState<IDatabaseTag>
    {
        public CreateTagState(ITogglApi api, IRepository<IDatabaseTag> repository) : base(api, repository)
        {
        }

        protected override IObservable<IDatabaseTag> Create(IDatabaseTag entity)
            => Api.Tags.Create(entity).Select(Tag.Clean);

        protected override IDatabaseTag CopyFrom(IDatabaseTag entity)
            => Tag.From(entity);
    }
}
