﻿// <auto-generated />
using System;
using Havensread.Data.Ingestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Havensread.Data.Migrations.Ingestion
{
    [DbContext(typeof(IngestionDbContext))]
    partial class IngestionDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("ingestion")
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Havensread.Data.Ingestion.IngestedDocument", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Source")
                        .HasColumnType("text")
                        .HasColumnName("source");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<int>("Version")
                        .HasColumnType("integer")
                        .HasColumnName("version");

                    b.HasKey("Id", "Source")
                        .HasName("pk_documents");

                    b.ToTable("documents", "ingestion");
                });

            modelBuilder.Entity("Havensread.Data.Ingestion.IngestedRecord", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid")
                        .HasColumnName("document_id");

                    b.Property<string>("DocumentSource")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("document_source");

                    b.HasKey("Id")
                        .HasName("pk_records");

                    b.HasIndex("DocumentId", "DocumentSource")
                        .HasDatabaseName("ix_records_document_id_document_source");

                    b.ToTable("records", "ingestion");
                });

            modelBuilder.Entity("Havensread.Data.Ingestion.IngestedRecord", b =>
                {
                    b.HasOne("Havensread.Data.Ingestion.IngestedDocument", null)
                        .WithMany("Records")
                        .HasForeignKey("DocumentId", "DocumentSource")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_records_documents_document_id_document_source");
                });

            modelBuilder.Entity("Havensread.Data.Ingestion.IngestedDocument", b =>
                {
                    b.Navigation("Records");
                });
#pragma warning restore 612, 618
        }
    }
}
