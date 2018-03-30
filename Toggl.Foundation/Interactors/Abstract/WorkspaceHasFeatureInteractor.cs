using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Interactors
{
    internal abstract class WorkspaceHasFeatureInteractor<TValue> : IInteractor<IObservable<TValue>>
    {
        protected ITogglDatabase Database { get; }

        public WorkspaceHasFeatureInteractor(ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(database, nameof(database));

            Database = database;
        }

        public IObservable<bool> CheckIfFeatureIsEnabled(long workspaceId, WorkspaceFeatureId featureId)
            => Database.WorkspaceFeatures
                .GetById(workspaceId)
                .Select(featureCollection => featureCollection.IsEnabled(featureId));

        public abstract IObservable<TValue> Execute();
    }
}
