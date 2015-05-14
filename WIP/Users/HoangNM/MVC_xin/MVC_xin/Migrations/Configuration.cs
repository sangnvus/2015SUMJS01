using MVC_xin.Models;

namespace MVC_xin.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MVC_xin.Models.MovieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MVC_xin.Models.MovieDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Movies.AddOrUpdate(i => i.Title,
       new Movie
       {
           Title = "When Harry Met Sally",
           ReleaseDate = DateTime.Parse("1989-1-11"),
           Genre = "Romantic Comedy",
           Price = 7.99M,
           Rating = "G"
       },

        new Movie
        {
            Title = "Ghostbusters ",
            ReleaseDate = DateTime.Parse("1984-3-13"),
            Genre = "Comedy",
            Price = 8.99M,
            Rating = "G"
        },

        new Movie
        {
            Title = "Ghostbusters 2",
            ReleaseDate = DateTime.Parse("1986-2-23"),
            Genre = "Comedy",
            Price = 9.99M,
            Rating = "G"
        },

      new Movie
      {
          Title = "Rio Bravo",
          ReleaseDate = DateTime.Parse("1959-4-15"),
          Genre = "Western",
          Price = 3.99M,
          Rating = "G"
      }
  );
        }
    }
}
