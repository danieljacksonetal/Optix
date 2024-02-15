using Optix.Movies.Infrastructure.Database.Entities;

namespace Optix.Movies.Infrastructure.Models
{
    public class MoviesResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<Movie> Movies { get; set; }

        public static MoviesResponse Create(List<Movie> movies, bool success = true, string error = null)
        {
            return new MoviesResponse()
            {
                Movies = movies,
                Success = success,
                Error = error
            };
        }
    }
}
