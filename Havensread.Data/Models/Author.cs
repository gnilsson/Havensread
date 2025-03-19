using System.ComponentModel.DataAnnotations;

namespace Havensread.Data;

public sealed class Author
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public ICollection<Book> Books { get; } = [];
}
