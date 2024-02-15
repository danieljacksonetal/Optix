namespace Optix.Movies.Infrastructure.Database.Entities
{
    public class Movie
    {
        public DateTime? ReleaseDate { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public Int16 VoteCount { get; set; }
        public double VoteAverage { get; set; }
        public string OriginalLanguage { get; set; }
        public string Genre { get; set; }

        public Int16 Id { get; set; }
        public string PosterUrl { get; set; }
    }
}
