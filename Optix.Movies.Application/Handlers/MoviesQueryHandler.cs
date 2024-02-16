using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Optix.Movies.Infrastructure.Database;
using Optix.Movies.Infrastructure.Database.Entities;
using System.Linq.Expressions;
using System.Reflection;

namespace Optix.Movies.Infrastructure.Models
{
    public class MoviesQueryHandler : IRequestHandler<MoviesQuery, MoviesResponse>
    {
        private readonly MoviesContext _context;
        private readonly ILogger<MoviesQueryHandler> _logger;

        public MoviesQueryHandler(MoviesContext context, ILogger<MoviesQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MoviesResponse> Handle(MoviesQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("query received {query}", JsonConvert.SerializeObject(query));

            List<Movie> response = null;
            try
            {

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    response = await _context.Movies.Where(x =>
                        x.Title != null && x.Title.ToLower().Contains(query.SearchTerm.ToLower()) ||
                        x.Overview != null && x.Overview.ToLower().Contains(query.SearchTerm.ToLower()) ||
                        x.Genre != null && x.Genre.ToLower().Contains(query.SearchTerm.ToLower())).ToListAsync(cancellationToken);
                }
                else
                {
                    response = await _context.Movies.ToListAsync(cancellationToken);
                }

                response = SortResponse(query, response);

                var paged = response.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

                return MoviesResponse.Create(paged);

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the query: {message}", ex.Message);

                // assuming we are going straight back to a client, dont expose the exception or message                
                return MoviesResponse.Create(new List<Movie>(), false, "An error occurred while processing the query");
            }
        }

        private static List<Movie> SortResponse(MoviesQuery query, List<Movie> currentResponse)
        {
            var orderExpression = ExtractOrderExpression(query);

            return (query.SortDirection, query.SortBy)
            switch
            {
                (SortDirection.Ascending, SortBy.OriginalLanguage) or (SortDirection.Ascending, SortBy.Overview) or (SortDirection.Ascending, SortBy.Genre) or (SortDirection.Ascending, SortBy.Title) => currentResponse.AsQueryable().OrderBy((Expression<Func<Movie, string>>)orderExpression).ToList(),
                (SortDirection.Descending, SortBy.OriginalLanguage) or (SortDirection.Descending, SortBy.Overview) or (SortDirection.Descending, SortBy.Genre) or (SortDirection.Descending, SortBy.Title) => currentResponse.AsQueryable().OrderByDescending((Expression<Func<Movie, string>>)orderExpression).ToList(),
                (SortDirection.Ascending, SortBy.VoteAverage) => currentResponse.AsQueryable().OrderBy((Expression<Func<Movie, double>>)orderExpression).ToList(),
                (SortDirection.Descending, SortBy.VoteAverage) => currentResponse.AsQueryable().OrderByDescending((Expression<Func<Movie, double>>)orderExpression).ToList(),
                (SortDirection.Ascending, SortBy.ReleaseDate) => currentResponse.AsQueryable().OrderBy((Expression<Func<Movie, DateTime?>>)orderExpression).ToList(),
                (SortDirection.Descending, SortBy.ReleaseDate) => currentResponse.AsQueryable().OrderByDescending((Expression<Func<Movie, DateTime?>>)orderExpression).ToList(),
                _ => currentResponse.AsQueryable().OrderBy((Expression<Func<Movie, string>>)orderExpression).ToList(),
            };
        }

        private static Expression ExtractOrderExpression(MoviesQuery query)
        {
            PropertyInfo property = typeof(Movie).GetProperty(query.SortBy.ToString(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var parameter = Expression.Parameter(typeof(Movie), "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            return lambda;
        }


    }
}