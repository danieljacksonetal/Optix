using MediatR;
using Optix.Movies.Infrastructure.Configuration;

namespace Optix.Movies.Infrastructure.Models
{
    public class MoviesQuery : IRequest<MoviesResponse>
    {
        public string SearchTerm { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }

        public SortDirection SortDirection { get; set; }
        public SortBy SortBy { get; set; }

        public void SetDefaults(IPageDefaults pageDefaults)
        {
            PageSize = PageSize == 0
                ? pageDefaults.DefaultPageSize
                : PageSize;
            PageSize = PageSize >= pageDefaults.MaxPageSize
                ? pageDefaults.MaxPageSize
                : PageSize;

            Page = Page >= 0 ? Page = 1 : Page;
        }
    }
}
