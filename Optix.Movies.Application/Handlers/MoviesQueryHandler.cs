using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Optix.Movies.Infrastructure.Database;
using Optix.Movies.Infrastructure.Database.Entities;
using PagedList.Core;
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

        public Task<MoviesResponse> Handle(MoviesQuery query, CancellationToken cancellationToken)
        {
            _logger.LogDebug("query received {query}", JsonConvert.SerializeObject(query));

            IQueryable<Movie> response = null;
            try
            {

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    response = _context.Movies.Where(x =>
                        x.Title != null && x.Title.ToLower().Contains(query.SearchTerm.ToLower()) ||
                        x.Overview != null && x.Overview.ToLower().Contains(query.SearchTerm.ToLower()) ||
                        x.Genre != null && x.Genre.ToLower().Contains(query.SearchTerm.ToLower()));
                }
                else
                {
                    response = _context.Movies;
                }

                var ordered = SortResponse(query, response);

                return Task.FromResult(MoviesResponse.Create(ordered.ToPagedList(query.Page - 1, query.PageSize)));

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing the query: {message}", ex.Message);

                // assuming we are going straight back to a client, dont expose the exception or message                
                return Task.FromResult(MoviesResponse.Create(new PagedList<Movie>(new List<Movie>(), query.Page - 1, query.PageSize), false, "An error occurred while processing the query"));
            }
        }

        private static IOrderedEnumerable<Movie> SortResponse(MoviesQuery query, IQueryable<Movie> currentResponse)
        {
            var orderExpression = ExtractOrderExpression(query);

            return query.SortDirection switch
            {
                SortDirection.Ascending => currentResponse.OrderBy(orderExpression.Compile()),
                SortDirection.Descending => currentResponse.OrderByDescending(orderExpression.Compile()),
                _ => currentResponse.OrderBy(orderExpression.Compile()),
            };
        }

        private static Expression<Func<Movie, object>> ExtractOrderExpression(MoviesQuery query)
        {
            PropertyInfo property = typeof(Movie).GetProperty(query.SortBy.ToString(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var parameter = Expression.Parameter(typeof(Movie), "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            return (Expression<Func<Movie, object>>)Expression.Lambda(Expression.Convert(propertyAccess, typeof(Object)).Reduce(), parameter);

        }


    }
}