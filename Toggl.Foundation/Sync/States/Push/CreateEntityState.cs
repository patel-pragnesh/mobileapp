using System;
using System.Reactive.Linq;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.ApiClients;
using static Toggl.Foundation.Sync.PushSyncOperation;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class CreateEntityState<TModel, TThreadsafeModel>
        : BasePushEntityState<TThreadsafeModel>
        where TModel : IIdentifiable
        where TThreadsafeModel : class, TModel, IThreadSafeModel
    {
        private readonly ICreatingApiClient<TModel> api;

        private readonly Func<TModel, TThreadsafeModel> convertToThreadsafeModel;

        public StateResult<TThreadsafeModel> CreatingFinished { get; } = new StateResult<TThreadsafeModel>();

        public CreateEntityState(
            ICreatingApiClient<TModel> api,
            IBaseDataSource<TThreadsafeModel> dataSource,
            IAnalyticsService analyticsService,
            Func<TModel, TThreadsafeModel> convertToThreadsafeModel)
            : base(dataSource, analyticsService)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(convertToThreadsafeModel, nameof(convertToThreadsafeModel));

            this.api = api;
            this.convertToThreadsafeModel = convertToThreadsafeModel;
        }

        public override IObservable<ITransition> Start(TThreadsafeModel entity)
            => create(entity)
                .SelectMany(Overwrite(entity))
                .Track(AnalyticsService.EntitySynced, Create, entity.GetSafeTypeName())
                .Track(AnalyticsService.EntitySyncStatus, entity.GetSafeTypeName(), $"{Create}:{Resources.Success}")
                .Select(CreatingFinished.Transition)
                .Catch(Fail(entity, Create));

        private IObservable<TThreadsafeModel> create(TThreadsafeModel entity)
            => entity == null
                ? Observable.Throw<TThreadsafeModel>(new ArgumentNullException(nameof(entity)))
                : api.Create(entity).Select(convertToThreadsafeModel);
    }
}
