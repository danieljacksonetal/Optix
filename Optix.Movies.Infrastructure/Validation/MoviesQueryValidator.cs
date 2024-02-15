using FluentValidation;
using Microsoft.Extensions.Options;
using Optix.Movies.Infrastructure.Configuration;
using Optix.Movies.Infrastructure.Models;
using System.Text;

namespace Optix.Movies.Infrastructure.Validation
{
    public class MoviesQueryValidator : AbstractValidator<MoviesQuery>
    {
        private readonly IPageDefaults _pageDefaults;
        public MoviesQueryValidator(IOptionsMonitor<PageDefaults> pageDefaults)
        {
            _pageDefaults = pageDefaults.CurrentValue;

            RuleFor(x => x).Must(SetDefaults);
            RuleFor(x => x).Must(RemoveSpecialCharacters);
        }

        private bool SetDefaults(MoviesQuery query)
        {
            query.SetDefaults(_pageDefaults);
            return true;
        }

        private bool RemoveSpecialCharacters(MoviesQuery query)
        {
            StringBuilder sb = new();
            foreach (char c in query.SearchTerm.ToLowerInvariant())
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            query.SearchTerm = sb.ToString();
            return true;
        }
    }
}
