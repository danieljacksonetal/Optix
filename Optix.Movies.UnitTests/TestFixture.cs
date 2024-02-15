﻿using AutoFixture;
using MockQueryable.NSubstitute;
using NSubstitute;
using Optix.Movies.Infrastructure.Database;
using Optix.Movies.Infrastructure.Database.Entities;
using Optix.Movies.Infrastructure.Models;

namespace Optix.Movies.UnitTests
{
    public class TestFixture
    {
        private readonly Fixture AutoFixture;

        public MoviesQuery MoviesQuery;
        public List<Movie> Movies;
        public MoviesContext MoviesContext;

        public TestFixture()
        {
            AutoFixture = new Fixture();
            MoviesContext = Substitute.For<MoviesContext>();

            Movies = CreateMovies();
            Movies.First().Title = "aTitle";
            Movies.Last().Title = "zTitle";

            Movies[2].Title = "SortedByGenre";
            Movies[2].Genre = "aaaaaaaaaaaaa";

            var moviesDbSet = Movies.AsQueryable().BuildMockDbSet();
            MoviesContext.Movies.Returns(moviesDbSet);

            MoviesQuery = new MoviesQuery()
            {
                SearchTerm = "Title",
                Page = 1,
                PageSize = 10,
                SortDirection = 0,
                SortBy = 0
            };
        }

        private List<Movie> CreateMovies() => AutoFixture.Build<Movie>().CreateMany(20).ToList();
    }
}