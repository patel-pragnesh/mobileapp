using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync
{
    internal sealed class StateMachine : IStateMachine
    {
        private readonly TimeSpan stateTimeout = TimeSpan.FromMinutes(1);

        private readonly Subject<StateMachineEvent> stateTransitions = new Subject<StateMachineEvent>();
        public IObservable<StateMachineEvent> StateTransitions { get; }

        private readonly ITransitionHandlerProvider transitionHandlerProvider;
        private readonly IScheduler scheduler;
        private readonly ISubject<Unit> delayCancellation;

        private bool isRunning;
        private bool isFrozen;

        public StateMachine(ITransitionHandlerProvider transitionHandlerProvider, IScheduler scheduler, ISubject<Unit> delayCancellation)
        {
            Ensure.Argument.IsNotNull(transitionHandlerProvider, nameof(transitionHandlerProvider));
            Ensure.Argument.IsNotNull(scheduler, nameof(scheduler));
            Ensure.Argument.IsNotNull(delayCancellation, nameof(delayCancellation));

            this.transitionHandlerProvider = transitionHandlerProvider;
            this.scheduler = scheduler;
            this.delayCancellation = delayCancellation;

            StateTransitions = stateTransitions.AsObservable();
            isFrozen = false;
        }

        public void Start(ITransition transition)
        {
            Ensure.Argument.IsNotNull(transition, nameof(transition));

            if (isRunning)
                throw new InvalidOperationException("Cannot start state machine if it is already running.");

            if (isFrozen)
                throw new InvalidOperationException("Cannot start state machine again if it was frozen.");

            start(transition);
        }

        public void Freeze()
        {
            delayCancellation.OnNext(Unit.Default);
            delayCancellation.OnCompleted();
            isFrozen = true;
        }

        private async void start(ITransition transition)
        {
            StateMachineEvent result = null;
            try
            {
                isRunning = true;
                result = await run(transition).ConfigureAwait(false);
            }
            finally
            {
                isRunning = false;
            }

            if (result == null)
            {
                var exception = new InvalidOperationException("State machine finished and did not return any valid event.");
                result = new StateMachineError(exception);
            }

            stateTransitions.OnNext(result);
        }

        private async Task<StateMachineEvent> run(ITransition transition)
        {
            var transitionHandler = getHandler(transition);
            while (transitionHandler != null && isFrozen == false)
            {
                stateTransitions.OnNext(new StateMachineTransition(transition));

                try
                {
                    transition = await transitionHandler(transition).SingleAsync();
                }
                catch (Exception exception)
                {
                    return new StateMachineError(exception);
                }

                transitionHandler = getHandler(transition);
            }

            return new StateMachineDeadEnd(transition);
        }

        private TransitionHandler getHandler(ITransition transition)
            => transitionHandlerProvider.GetTransitionHandler(transition.Result);
    }
}
