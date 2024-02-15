using Microsoft.EntityFrameworkCore;
using Optix.Movies.Infrastructure.Database.Configurations;
using Optix.Movies.Infrastructure.Database.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Optix.Movies.Infrastructure.Database
{
    [ExcludeFromCodeCoverage]
    public class MoviesContext : DbContext
    {
        public virtual DbSet<Movie> Movies { get; set; }
        public MoviesContext()
        { }

        public MoviesContext(DbContextOptions<MoviesContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MovieConfiguration());
        }
    }
}
