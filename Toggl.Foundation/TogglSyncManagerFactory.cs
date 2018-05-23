using System;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using System.Reactive.Concurrency;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Toggl.Foundation.DataSources;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation
{
    public static class TogglSyncManager
    {
        public static ISyncManager CreateSyncManager(
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IAnalyticsService analyticsService,
            TimeSpan? retryLimit,
            IScheduler scheduler)
        {
            var random = new Random();
            var queue = new SyncStateQueue();
            var entryPoints = new StateMachineEntryPoints();
            var transitions = new TransitionHandlerProvider();
            var apiDelay = new RetryDelayService(random, retryLimit);
            var delayCancellation = new Subject<Unit>();
            var delayCancellationObservable = delayCancellation.AsObservable().Replay();
            ConfigureTransitions(transitions, database, api, dataSource, apiDelay, scheduler, timeService, entryPoints, delayCancellationObservable);
            var stateMachine = new StateMachine(transitions, scheduler, delayCancellation);
            var orchestrator = new StateMachineOrchestrator(stateMachine, entryPoints);

            return new SyncManager(queue, orchestrator, analyticsService);
        }

        internal static void ConfigureTransitions(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            ITimeService timeService,
            StateMachineEntryPoints entryPoints,
            IObservable<Unit> delayCancellation)
        {
            configurePullTransitions(transitions, database, api, dataSource, timeService, scheduler, entryPoints.StartPullSync, delayCancellation);
            configurePushTransitions(transitions, database, api, dataSource, apiDelay, scheduler, entryPoints.StartPushSync, delayCancellation);
        }

        private static void configurePullTransitions(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            ITimeService timeService,
            IScheduler scheduler,
            StateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var fetchAllSince = new FetchAllSinceState(database, api, timeService);
            var persistWorkspaces = new PersistWorkspacesState(database.Workspaces, database.SinceParameters);
            var persistWorkspaceFeatures = new PersistWorkspacesFeaturesState(database.WorkspaceFeatures, database.SinceParameters);
            var persistUser = new PersistUserState(database.User, database.SinceParameters);
            var persistTags = new PersistTagsState(database.Tags, database.SinceParameters);
            var persistClients = new PersistClientsState(database.Clients, database.SinceParameters);
            var persistPreferences = new PersistPreferencesState(dataSource.Preferences, database.SinceParameters);
            var persistProjects = new PersistProjectsState(database.Projects, database.SinceParameters);
            var persistTimeEntries = new PersistTimeEntriesState(dataSource.TimeEntries, database.SinceParameters, timeService);
            var persistTasks = new PersistTasksState(database.Tasks, database.SinceParameters);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            transitions.ConfigureTransition(entryPoint, fetchAllSince);
            transitions.ConfigureTransition(fetchAllSince.FetchStarted, persistWorkspaces);
            transitions.ConfigureTransition(persistWorkspaces.FinishedPersisting, persistUser);
            transitions.ConfigureTransition(persistUser.FinishedPersisting, persistWorkspaceFeatures);
            transitions.ConfigureTransition(persistWorkspaceFeatures.FinishedPersisting, persistPreferences);
            transitions.ConfigureTransition(persistPreferences.FinishedPersisting, persistTags);
            transitions.ConfigureTransition(persistTags.FinishedPersisting, persistClients);
            transitions.ConfigureTransition(persistClients.FinishedPersisting, persistProjects);
            transitions.ConfigureTransition(persistProjects.FinishedPersisting, persistTasks);
            transitions.ConfigureTransition(persistTasks.FinishedPersisting, persistTimeEntries);

            transitions.ConfigureTransition(persistWorkspaces.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistWorkspaceFeatures.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistPreferences.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistTags.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistClients.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistProjects.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistTasks.Failed, checkServerStatus);
            transitions.ConfigureTransition(persistTimeEntries.Failed, checkServerStatus);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, finished);
            transitions.ConfigureTransition(finished.Continue, fetchAllSince);
        }

        private static void configurePushTransitions(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var pushingUsersFinished = configurePushTransitionsForUsers(transitions, database, api, scheduler, entryPoint, delayCancellation);
            var pushingPreferencesFinished = configurePushTransitionsForPreferences(transitions, database, api, scheduler, pushingUsersFinished, delayCancellation);
            var pushingTagsFinished = configurePushTransitionsForTags(transitions, database, api, scheduler, pushingPreferencesFinished, delayCancellation);
            var pushingClientsFinished = configurePushTransitionsForClients(transitions, database, api, scheduler, pushingTagsFinished, delayCancellation);
            var pushingProjectsFinished = configurePushTransitionsForProjects(transitions, database, api, scheduler, pushingClientsFinished, delayCancellation);
            configurePushTransitionsForTimeEntries(transitions, database, api, dataSource, apiDelay, scheduler, pushingProjectsFinished, delayCancellation);
        }

        private static IStateResult configurePushTransitionsForTimeEntries(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            ITogglDataSource dataSource,
            IRetryDelayService apiDelay,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushTimeEntriesState(database.TimeEntries);
            var pushOne = new PushOneEntityState<IDatabaseTimeEntry>();
            var create = new CreateTimeEntryState(api, dataSource.TimeEntries);
            var update = new UpdateTimeEntryState(api, dataSource.TimeEntries);
            var delete = new DeleteTimeEntryState(api, database.TimeEntries);
            var deleteLocal = new DeleteLocalTimeEntryState(database.TimeEntries);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabaseTimeEntry>();
            var unsyncable = new UnsyncableTimeEntryState(dataSource.TimeEntries);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configurePush(transitions, entryPoint, push, pushOne, create, update, delete, deleteLocal, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForTags(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushTagsState(database.Tags);
            var pushOne = new PushOneEntityState<IDatabaseTag>();
            var create = new CreateTagState(api, database.Tags);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabaseTag>();
            var unsyncable = new UnsyncableTagState(database.Tags);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForClients(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushClientsState(database.Clients);
            var pushOne = new PushOneEntityState<IDatabaseClient>();
            var create = new CreateClientState(api, database.Clients);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabaseClient>();
            var unsyncable = new UnsyncableClientState(database.Clients);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForProjects(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushProjectsState(database.Projects);
            var pushOne = new PushOneEntityState<IDatabaseProject>();
            var create = new CreateProjectState(api, database.Projects);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabaseProject>();
            var unsyncable = new UnsyncableProjectState(database.Projects);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureCreateOnlyPush(transitions, entryPoint, push, pushOne, create, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForUsers(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushUsersState(database.User);
            var pushOne = new PushOneEntityState<IDatabaseUser>();
            var update = new UpdateUserState(api, database.User);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabaseUser>();
            var unsyncable = new UnsyncableUserState(database.User);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureUpdateOnlyPush(transitions, entryPoint, push, pushOne, update, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePushTransitionsForPreferences(
            ITransitionConfigurator transitions,
            ITogglDatabase database,
            ITogglApi api,
            IScheduler scheduler,
            IStateResult entryPoint,
            IObservable<Unit> delayCancellation)
        {
            var rnd = new Random();
            var apiDelay = new RetryDelayService(rnd);
            var statusDelay = new RetryDelayService(rnd);

            var push = new PushPreferencesState(database.Preferences);
            var pushOne = new PushOneEntityState<IDatabasePreferences>();
            var update = new UpdatePreferencesState(api, database.Preferences);
            var tryResolveClientError = new TryResolveClientErrorState<IDatabasePreferences>();
            var unsyncable = new UnsyncablePreferencesState(database.Preferences);
            var checkServerStatus = new CheckServerStatusState(api, scheduler, apiDelay, statusDelay, delayCancellation);
            var finished = new ResetAPIDelayState(apiDelay);

            return configureUpdateOnlyPush(transitions, entryPoint, push, pushOne, update, tryResolveClientError, unsyncable, checkServerStatus, finished);
        }

        private static IStateResult configurePush<T>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            BasePushState<T> push,
            PushOneEntityState<T> pushOne,
            BaseCreateEntityState<T> create,
            BaseUpdateEntityState<T> update,
            BaseDeleteEntityState<T> delete,
            BaseDeleteLocalEntityState<T> deleteLocal,
            TryResolveClientErrorState<T> tryResolveClientError,
            BaseUnsyncableEntityState<T> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where T : class, IBaseModel, IDatabaseSyncable
        {
            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.CreateEntity, create);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update);
            transitions.ConfigureTransition(pushOne.DeleteEntity, delete);
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, deleteLocal);

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(update.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(delete.ClientError, tryResolveClientError);

            transitions.ConfigureTransition(create.ServerError, checkServerStatus);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus);
            transitions.ConfigureTransition(delete.ServerError, checkServerStatus);

            transitions.ConfigureTransition(create.UnknownError, checkServerStatus);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus);
            transitions.ConfigureTransition(delete.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(create.CreatingFinished, finished);
            transitions.ConfigureTransition(update.UpdatingSucceeded, finished);
            transitions.ConfigureTransition(delete.DeletingFinished, finished);
            transitions.ConfigureTransition(deleteLocal.Deleted, finished);
            transitions.ConfigureTransition(deleteLocal.DeletingFailed, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }

        private static IStateResult configureCreateOnlyPush<T>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            BasePushState<T> push,
            PushOneEntityState<T> pushOne,
            BaseCreateEntityState<T> create,
            TryResolveClientErrorState<T> tryResolveClientError,
            BaseUnsyncableEntityState<T> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where T : class, IBaseModel, IDatabaseSyncable
        {
            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.CreateEntity, create);

            transitions.ConfigureTransition(pushOne.UpdateEntity, new InvalidTransitionState($"Updating is not supported for {typeof(T).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(T).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(T).Name} during Push sync."));

            transitions.ConfigureTransition(create.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(create.ServerError, checkServerStatus);
            transitions.ConfigureTransition(create.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(create.CreatingFinished, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }

        private static IStateResult configureUpdateOnlyPush<T>(
            ITransitionConfigurator transitions,
            IStateResult entryPoint,
            BasePushState<T> push,
            PushOneEntityState<T> pushOne,
            BaseUpdateEntityState<T> update,
            TryResolveClientErrorState<T> tryResolveClientError,
            BaseUnsyncableEntityState<T> markUnsyncable,
            CheckServerStatusState checkServerStatus,
            ResetAPIDelayState finished)
            where T : class, IBaseModel, IDatabaseSyncable
        {
            transitions.ConfigureTransition(entryPoint, push);
            transitions.ConfigureTransition(push.PushEntity, pushOne);
            transitions.ConfigureTransition(pushOne.UpdateEntity, update);

            transitions.ConfigureTransition(pushOne.CreateEntity, new InvalidTransitionState($"Creating is not supported for {typeof(T).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntity, new InvalidTransitionState($"Deleting is not supported for {typeof(T).Name} during Push sync."));
            transitions.ConfigureTransition(pushOne.DeleteEntityLocally, new InvalidTransitionState($"Deleting locally is not supported for {typeof(T).Name} during Push sync."));

            transitions.ConfigureTransition(update.ClientError, tryResolveClientError);
            transitions.ConfigureTransition(update.ServerError, checkServerStatus);
            transitions.ConfigureTransition(update.UnknownError, checkServerStatus);

            transitions.ConfigureTransition(tryResolveClientError.UnresolvedTooManyRequests, checkServerStatus);
            transitions.ConfigureTransition(tryResolveClientError.Unresolved, markUnsyncable);

            transitions.ConfigureTransition(checkServerStatus.Retry, checkServerStatus);
            transitions.ConfigureTransition(checkServerStatus.ServerIsAvailable, push);

            transitions.ConfigureTransition(update.UpdatingSucceeded, finished);

            transitions.ConfigureTransition(finished.Continue, push);

            return push.NothingToPush;
        }
    }
}
