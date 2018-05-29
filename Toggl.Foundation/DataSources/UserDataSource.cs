using System;
using System.Reactive.Linq;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class UserDataSource
        : SingletonDataSource<IThreadSafeUser, IDatabaseUser>, IUserSource
    {
        private readonly ITimeService timeService;

        public UserDataSource(ISingleObjectStorage<IDatabaseUser> storage, ITimeService timeService)
            : base(storage, null)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.timeService = timeService;
        }

        public IObservable<IThreadSafeUser> UpdateWorkspace(long workspaceId)
            => Get()
                .Select(user => ToDatabase(user).With(workspaceId))
                .SelectMany(Update);

        public IObservable<IThreadSafeUser> Update(EditUserDTO dto)
            => Get()
                .Select(user => updatedUser(user, dto))
                .SelectMany(Update);

        private IThreadSafeUser updatedUser(IThreadSafeUser existing, EditUserDTO dto)
            => User.Builder
                   .FromExisting(existing)
                   .SetBeginningOfWeek(dto.BeginningOfWeek)
                   .SetSyncStatus(SyncStatus.SyncNeeded)
                   .SetAt(timeService.CurrentDateTime)
                   .Build();

        protected override IThreadSafeUser Convert(IDatabaseUser entity)
            => User.From(entity);

        protected override IDatabaseUser ToDatabase(IThreadSafeUser entity)
        {
            throw new NotImplementedException();
        }

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseUser first, IDatabaseUser second)
            => Resolver.ForUser.Resolve(first, second);
    }
}
