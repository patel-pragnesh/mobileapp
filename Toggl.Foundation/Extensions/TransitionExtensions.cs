using System;
using Toggl.Foundation.Sync;

namespace Toggl.Foundation.Extensions
{
    public static class TransitionExtensions
    {
        public static T FindParameter<T>(this ITransition transition)
        {
            if (transition is Transition<T> concreteTransition)
                return concreteTransition.Parameter;

            throw new InvalidOperationException($"The given transition does not contain a parameter of type {typeof(T)}.");
        }
    }
}
