using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Sync.States.Pull;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;

namespace Toggl.Foundation.Sync.EntryPoints
{
    public sealed class PullSyncEntryPointFactory : IEntryPointFactory
    {
        private const int sinceDateLimitMonths = 2;

        private readonly ITogglDatabase database;

        private readonly ITogglApi api;

        private readonly ITimeService timeService;

        public PullSyncEntryPointFactory(ITogglDatabase database, ITogglApi api, ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(timeService, nameof(ITimeService));

            this.database = database;
            this.api = api;
            this.timeService = timeService;
        }


        public IState Create()
            => createPullWorkspaceState();

        private IState createPullWorkspaceState()
        {
            var pullWorkspaceFeatures = createPullWorkspaceFeaturesState();
            return new PullState<IWorkspace, IDatabaseWorkspace>(
                api.Workspaces.GetAll, database.Workspaces, Workspace.Clean, pullWorkspaceFeatures);
        }

        private IState createPullWorkspaceFeaturesState()
        {
            var pullUser = createPullUserState();
            return new PullState<IWorkspaceFeatureCollection, IDatabaseWorkspaceFeatureCollection>(
                api.WorkspaceFeatures.GetAll, database.WorkspaceFeatures, WorkspaceFeatureCollection.From, pullUser);
        }

        private IState createPullUserState()
        {
            var pullPreferences = createPullPreferencesState();
            return new PullState<IUser, IDatabaseUser>(
                asList(api.User.Get), database.User, User.Clean, pullPreferences);
        }

        private IState createPullPreferencesState()
        {
            var pullTags = createPullTagsState();
            return new PullState<IPreferences, IDatabasePreferences>(
                asList(api.Preferences.Get), database.Preferences, Preferences.Clean, pullTags);
        }

        private IState createPullTagsState()
        {
            var pullClients = createPullClientsState();
            return new PullSinceState<ITag, IDatabaseTag>(
                allOrSince(api.Tags.GetAll, api.Tags.GetAllSince),
                database.Tags, Tag.Clean, database.SinceParameters, pullClients);
        }

        private IState createPullClientsState()
        {
            var pullProjects = createPullProjectsState();
            return new PullSinceState<IClient, IDatabaseClient>(
                allOrSince(api.Clients.GetAll, api.Clients.GetAllSince),
                database.Clients, Client.Clean, database.SinceParameters, pullProjects);
        }

        private IState createPullProjectsState()
        {
            var pullTasks = createPullTasksState();
            return new PullSinceState<IProject, IDatabaseProject>(
                allOrSince(api.Projects.GetAll, api.Projects.GetAllSince),
                database.Projects, Project.Clean, database.SinceParameters, pullTasks);
        }

        private IState createPullTasksState()
        {
            var pullTimeEntries = createPullTimeEntriesState();
            return new PullSinceState<ITask, IDatabaseTask>(
                allOrSince(api.Tasks.GetAll, api.Tasks.GetAllSince),
                database.Tasks, Task.Clean, database.SinceParameters, pullTimeEntries);
        }

        private IState createPullTimeEntriesState()
            => new PullSinceState<ITimeEntry, IDatabaseTimeEntry>(
                allOrSince(api.TimeEntries.GetAll, api.TimeEntries.GetAllSince),
                database.TimeEntries, TimeEntry.Clean, database.SinceParameters);

        private Func<DateTimeOffset?, IObservable<IEnumerable<T>>> allOrSince<T>(
            Func<IObservable<IEnumerable<T>>> all,
            Func<DateTimeOffset, IObservable<IEnumerable<T>>> allSince)
            => since => since.HasValue && isWithinLimit(timeService, since.Value) ? allSince(since.Value) : all();

        private static bool isWithinLimit(ITimeService timeService, DateTimeOffset threshold)
            => threshold > timeService.CurrentDateTime.AddMonths(-sinceDateLimitMonths);

        private static Func<IObservable<IEnumerable<T>>> asList<T>(Func<IObservable<T>> getSingle)
            => () => getSingle().Select(entity => new List<T> { entity });
    }
}
