using System;
using Toggl.Multivac.Models;
using System.Collections.Generic;

namespace Toggl.Foundation.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<List<ICountry>>> GetAllCountries()
            => new GetAllCountriesInteractor();
    }
}
