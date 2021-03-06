﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Pull
{
    internal sealed class PersistSingletonState<TInterface, TDatabaseInterface, TThreadsafeInterface>
        : IPersistState
        where TDatabaseInterface : TInterface, IDatabaseSyncable
        where TThreadsafeInterface : class, TDatabaseInterface, IThreadSafeModel
    {
        private readonly ISingletonDataSource<TThreadsafeInterface> dataSource;

        private readonly Func<TInterface, TThreadsafeInterface> convertToThreadsafeEntity;

        public StateResult<IFetchObservables> FinishedPersisting { get; } = new StateResult<IFetchObservables>();

        public StateResult<ApiException> ErrorOccured { get; } = new StateResult<ApiException>();

        public PersistSingletonState(
            ISingletonDataSource<TThreadsafeInterface> dataSource,
            Func<TInterface, TThreadsafeInterface> convertToThreadsafeEntity)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(convertToThreadsafeEntity, nameof(convertToThreadsafeEntity));

            this.dataSource = dataSource;
            this.convertToThreadsafeEntity = convertToThreadsafeEntity;
        }

        public IObservable<ITransition> Start(IFetchObservables fetch)
            => fetch.GetSingle<TInterface>()
                .SingleAsync()
                .SelectMany(entity => entity == null
                    ? Observable.Return(Unit.Default)
                    : dataSource.UpdateWithConflictResolution(convertToThreadsafeEntity(entity)).Select(_ => Unit.Default))
                .Select(_ => FinishedPersisting.Transition(fetch))
                .OnErrorReturnResult(ErrorOccured);
    }
}
