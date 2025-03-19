using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Havensread.Data;

public sealed class Book
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required int SourceId { get; init; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required float AverageRating { get; set; }

    [Required]
    public int RatingsCount { get; set; }

    [Required]
    public int TextReviewsCount { get; set; }

    public string? ISBN { get; set; }

    public string? ISBN13 { get; set; }

    public string? LanguageCode { get; set; }

    public int? NumPages { get; set; }

    public required DateTime? PublicationDate { get; set; }

    public string? Publisher { get; set; }

    public ICollection<Author> Authors { get; init; } = [];

    public ICollection<Genre> Genres { get; init; } = [];
}
