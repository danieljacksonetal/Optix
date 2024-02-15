using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Optix.Movies.Infrastructure.Models;
using Shouldly;
using SortDirection = Optix.Movies.Infrastructure.Models.SortDirection;

namespace Optix.Movies.UnitTests.Application.Handlers
{
    public class MoviesQueryHandlerTests : TestFixture
    {
        private readonly MoviesQueryHandler _handler;

        public MoviesQueryHandlerTests()
        {
            var logger = Substitute.For<ILogger<MoviesQueryHandler>>();
            _handler = new MoviesQueryHandler(MoviesContext, logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task Handle_Returns_Movies_Response_Paged(int pageSize)
        {
            MoviesQuery.PageSize = pageSize;
            var response = await _handler.Handle(MoviesQuery, default);

            response.ShouldNotBeNull();
            response.Movies.Count.ShouldBe(pageSize);
            response.Success.ShouldBeTrue();
            response.Error.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_Should_Default_Order_Title_Asc()
        {
            MoviesQuery.PageSize = 20;

            var response = await _handler.Handle(MoviesQuery, default);

            response.ShouldNotBeNull();
            response.Movies.First().Title.ShouldBe("aTitle");
            response.Movies.Last().Title.ShouldBe("zTitle");
        }

        [Theory]
        [InlineData(SortDirection.Descending, SortBy.Genre)]
        [InlineData(SortDirection.Ascending, SortBy.Genre)]
        public async Task Handle_Should_Order_By_Query_SortBy_Direction(SortDirection sortDirection, SortBy sortBy)
        {
            MoviesQuery.SortDirection = sortDirection;
            MoviesQuery.SortBy = sortBy;
            MoviesQuery.SearchTerm = null;
            MoviesQuery.PageSize = 20;

            var response = await _handler.Handle(MoviesQuery, default);

            response.ShouldNotBeNull();
            if (sortDirection == SortDirection.Ascending)
            {
                response.Movies.First().Title.ShouldBe("SortedByGenre");
            }
            else
            {
                response.Movies.Last().Title.ShouldBe("SortedByGenre");
            }
        }

        [Fact]
        public async Task Handle_Returns_MoviesResponse_Even_If_Error()
        {
            MoviesContext.Movies.Throws(new Exception("Error"));

            var response = await _handler.Handle(MoviesQuery, default);
            response.ShouldNotBeNull();
            response.Movies.Count.ShouldBe(0);
            response.Success.ShouldBeFalse();
            response.Error.ShouldNotBe("Error");
            response.Error.ShouldBe("An error occurred while processing the query");
        }
    }
}
