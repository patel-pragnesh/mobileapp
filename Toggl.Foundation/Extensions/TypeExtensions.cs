using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Extensions
{
    public static class TypeExtensions
    {
        public static string GetSafeTypeName<T>(this T item) 
            => item?.GetType().Name ?? "[null]";
    }
}
