using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Havensread.Data.App;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app");

        modelBuilder.Entity<Book>(book =>
        {
            book.HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity(ba => ba.ToTable("books_authors"));

            book.HasMany(b => b.Genres)
            .WithMany(g => g.Books)
            .UsingEntity(bg => bg.ToTable("books_genres"));
        });

        modelBuilder.Entity<Author>(author =>
        {
            author.HasMany(a => a.Books)
            .WithMany(b => b.Authors)
            .UsingEntity(ab => ab.ToTable("books_authors"));
        });

        modelBuilder.Entity<Genre>(genre =>
        {
            genre.HasMany(g => g.Books)
            .WithMany(b => b.Genres)
            .UsingEntity(bg => bg.ToTable("books_genres"));
        });
    }
}
