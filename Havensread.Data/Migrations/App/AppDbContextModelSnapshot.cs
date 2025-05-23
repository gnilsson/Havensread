﻿// <auto-generated />
using System;
using Havensread.Data.App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Havensread.Data.Migrations.App
{
    [DbContext(typeof(AppDbContext))]
    partial class AppContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("app")
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AuthorBook", b =>
                {
                    b.Property<Guid>("AuthorsId")
                        .HasColumnType("uuid")
                        .HasColumnName("authors_id");

                    b.Property<Guid>("BooksId")
                        .HasColumnType("uuid")
                        .HasColumnName("books_id");

                    b.HasKey("AuthorsId", "BooksId")
                        .HasName("pk_books_authors");

                    b.HasIndex("BooksId")
                        .HasDatabaseName("ix_books_authors_books_id");

                    b.ToTable("books_authors", "app");
                });

            modelBuilder.Entity("BookGenre", b =>
                {
                    b.Property<Guid>("BooksId")
                        .HasColumnType("uuid")
                        .HasColumnName("books_id");

                    b.Property<Guid>("GenresId")
                        .HasColumnType("uuid")
                        .HasColumnName("genres_id");

                    b.HasKey("BooksId", "GenresId")
                        .HasName("pk_books_genres");

                    b.HasIndex("GenresId")
                        .HasDatabaseName("ix_books_genres_genres_id");

                    b.ToTable("books_genres", "app");
                });

            modelBuilder.Entity("Havensread.Data.Author", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_authors");

                    b.ToTable("authors", "app");
                });

            modelBuilder.Entity("Havensread.Data.Book", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<float>("AverageRating")
                        .HasColumnType("real")
                        .HasColumnName("average_rating");

                    b.Property<string>("ISBN")
                        .HasColumnType("text")
                        .HasColumnName("isbn");

                    b.Property<string>("ISBN13")
                        .HasColumnType("text")
                        .HasColumnName("isbn13");

                    b.Property<string>("LanguageCode")
                        .HasColumnType("text")
                        .HasColumnName("language_code");

                    b.Property<int?>("NumPages")
                        .HasColumnType("integer")
                        .HasColumnName("num_pages");

                    b.Property<DateTime?>("PublicationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("publication_date");

                    b.Property<string>("Publisher")
                        .HasColumnType("text")
                        .HasColumnName("publisher");

                    b.Property<int>("RatingsCount")
                        .HasColumnType("integer")
                        .HasColumnName("ratings_count");

                    b.Property<int>("SourceId")
                        .HasColumnType("integer")
                        .HasColumnName("source_id");

                    b.Property<int>("TextReviewsCount")
                        .HasColumnType("integer")
                        .HasColumnName("text_reviews_count");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_books");

                    b.ToTable("books", "app");
                });

            modelBuilder.Entity("Havensread.Data.Genre", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_genres");

                    b.ToTable("genres", "app");
                });

            modelBuilder.Entity("AuthorBook", b =>
                {
                    b.HasOne("Havensread.Data.Author", null)
                        .WithMany()
                        .HasForeignKey("AuthorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_books_authors_authors_authors_id");

                    b.HasOne("Havensread.Data.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_books_authors_books_books_id");
                });

            modelBuilder.Entity("BookGenre", b =>
                {
                    b.HasOne("Havensread.Data.Book", null)
                        .WithMany()
                        .HasForeignKey("BooksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_books_genres_books_books_id");

                    b.HasOne("Havensread.Data.Genre", null)
                        .WithMany()
                        .HasForeignKey("GenresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_books_genres_genres_genres_id");
                });
#pragma warning restore 612, 618
        }
    }
}
