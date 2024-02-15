using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using NSubstitute;
using Optix.Movies.Infrastructure.Configuration;
using Optix.Movies.Infrastructure.Validation;
using Shouldly;

namespace Optix.Movies.UnitTests.Infrastructure.Validation
{
    public class MoviesQueryValidatorTests : TestFixture
    {
        private readonly MoviesQueryValidator _validator;
        private readonly PageDefaults _pageDefaults;

        public MoviesQueryValidatorTests()
        {
            _pageDefaults = new PageDefaults
            {
                DefaultPageSize = 10,
                MaxPageSize = 100
            };
            var optionsMonitor = GetOptionsMonitor(_pageDefaults);
            _validator = new MoviesQueryValidator(optionsMonitor);
        }

        public static IOptionsMonitor<PageDefaults> GetOptionsMonitor(PageDefaults defaults)
        {
            var optionsMonitorMock = Substitute.For<IOptionsMonitor<PageDefaults>>();
            optionsMonitorMock.CurrentValue.Returns(defaults);
            return optionsMonitorMock;
        }

        [Fact]
        public void No_Defaults_Sets_Defaults()
        {
            MoviesQuery.Page = 0;
            MoviesQuery.PageSize = 0;

            _validator.TestValidate(MoviesQuery);

            MoviesQuery.Page.ShouldBe(1);
            MoviesQuery.PageSize.ShouldBe(_pageDefaults.DefaultPageSize);
        }

        [Fact]
        public void Should_Limit_Page_Size()
        {
            MoviesQuery.Page = 0;
            MoviesQuery.PageSize = 200;

            _validator.TestValidate(MoviesQuery);

            MoviesQuery.Page.ShouldBe(1);
            MoviesQuery.PageSize.ShouldBe(_pageDefaults.MaxPageSize);
        }

        [Fact]
        public void Should_Remove_Special_Characters()
        {
            MoviesQuery.SearchTerm = "a!@#$%^&*()_+1234567890";

            _validator.TestValidate(MoviesQuery);

            MoviesQuery.SearchTerm.ShouldBe("a_1234567890");
        }

    }
}
