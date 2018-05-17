using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;
using Toggl.Multivac.Models;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectCountryViewModelTests
    {
        public abstract class SelectCountryViewModelTest : BaseViewModelTests<SelectCountryViewModel>
        {
            protected SelectCountryParameter Parameters { get; }
                = SelectCountryParameter.With("", "");

            protected override SelectCountryViewModel CreateViewModel()
                => new SelectCountryViewModel(InteractorFactory, NavigationService);

            protected List<ICountry> GenerateCountriesList() =>
                Enumerable.Range(1, 10).Select(i =>
                {
                    var country = Substitute.For<ICountry>();
                    country.Id.Returns(i);
                    country.Name.Returns(i.ToString());
                    country.CountryCode.Returns(i.ToString());
                    return country;
                }).ToList();
        }

        public sealed class TheConstructor : SelectCountryViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useInteractorFactory, bool useNavigationService)
            {
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var navigationService = useNavigationService ? NavigationService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SelectCountryViewModel(interactorFactory, navigationService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheInitializeMethod : SelectCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task AddsAllCountriesToTheListOfSuggestions()
            {
                var countries = GenerateCountriesList();

                InteractorFactory.GetAllCountries()
                    .Execute()
                    .Returns(Observable.Return(countries));
                
                ViewModel.Prepare(Parameters);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Should().HaveCount(10);
            }

            [Theory, LogIfTooSlow]
            [InlineData("1")]
            [InlineData("2")]
            [InlineData("3")]
            [InlineData("4")]
            [InlineData("5")]
            [InlineData("6")]
            [InlineData("7")]
            [InlineData("8")]
            [InlineData("9")]
            public async Task SetsTheAppropriateCountryAsTheCurrentlySelectedOne(string code)
            {
                var parameter = SelectCountryParameter.With(code, "");
                var countries = GenerateCountriesList();
                InteractorFactory.GetAllCountries()
                    .Execute()
                    .Returns(Observable.Return(countries));
                ViewModel.Prepare(parameter);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Single(c => c.Selected).Country.CountryCode.Should().Be(code);
            }
        }

        public sealed class TheSelectCountryCommand : SelectCountryViewModelTest
        {
            private const string countryCode = "AL";

            public TheSelectCountryCommand()
            {
                var countries = GenerateCountriesList();
                InteractorFactory.GetAllCountries()
                    .Execute()
                    .Returns(Observable.Return(countries));
                ViewModel.Prepare(Parameters);
            }

            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModelWithSelectedCountryCode()
            {
                var country = Substitute.For<ICountry>();
                country.Id.Returns(1);
                country.Name.Returns(countryCode);
                country.CountryCode.Returns(countryCode);

                await ViewModel.Initialize();

                var selectableCountry = new SelectableCountryViewModel(country, true);

                ViewModel.SelectCountryCommand.Execute(selectableCountry);

                await NavigationService.Received()
                   .Close(Arg.Is(ViewModel), Arg.Any<SelectCountryParameter>());
            }
        }

        public sealed class TheTextProperty : SelectCountryViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task FiltersTheSuggestionsWhenItChanges()
            {
                var countries = GenerateCountriesList();
                InteractorFactory.GetAllCountries()
                    .Execute()
                    .Returns(Observable.Return(countries));
                ViewModel.Prepare(Parameters);
                await ViewModel.Initialize();

                ViewModel.Text = "2";

                ViewModel.Suggestions.Should().HaveCount(1);
            }
        }
    }
}
