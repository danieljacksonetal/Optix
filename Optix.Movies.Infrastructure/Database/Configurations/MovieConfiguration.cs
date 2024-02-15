using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optix.Movies.Infrastructure.Database.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Optix.Movies.Infrastructure.Database.Configurations
{
    [ExcludeFromCodeCoverage]
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToTable("movies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.ReleaseDate)
                .HasColumnName("Release_Date");

            builder.Property(e => e.Title)
                .HasColumnName("Title");

            builder.Property(e => e.Overview)
                .HasColumnName("Overview");

            builder.Property(e => e.Popularity)
                .HasColumnName("Popularity");

            builder.Property(e => e.VoteCount)
                .HasColumnName("Vote_Count");

            builder.Property(e => e.VoteAverage)
                .HasColumnName("Vote_Average");

            builder.Property(e => e.OriginalLanguage)
                .HasColumnName("Original_Language");

            builder.Property(e => e.Genre)
                .HasColumnName("Genre");

            builder.Property(e => e.PosterUrl)
                .HasColumnName("Poster_Url");
        }
    }
}
