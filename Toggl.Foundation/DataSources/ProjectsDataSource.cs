using System;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public sealed class ProjectsDataSource
        : DataSource<IThreadSafeProject, IDatabaseProject>, IProjectsSource
    {
        private readonly IIdProvider idProvider;
        private readonly ITimeService timeService;

        public ProjectsDataSource(IIdProvider idProvider, IRepository<IDatabaseProject> repository, ITimeService timeService)
            : base(repository)
        {
            Ensure.Argument.IsNotNull(idProvider, nameof(idProvider));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.idProvider = idProvider;
            this.timeService = timeService;
        }

        public IObservable<IDatabaseProject> Create(CreateProjectDTO dto)
        {
            var project = new Project(
                idProvider.GetNextIdentifier(),
                dto.Name,
                timeService.CurrentDateTime,
                SyncStatus.SyncNeeded,
                dto.Color,
                dto.WorkspaceId,
                dto.ClientId,
                billable: dto.Billable
            );

            return Create(project);
        }

        protected override IThreadSafeProject Convert(IDatabaseProject entity)
            => Project.From(entity);

        protected override ConflictResolutionMode ResolveConflicts(IDatabaseProject first, IDatabaseProject second)
            => Resolver.ForProjects.Resolve(first, second);
    }
}
