using System.ComponentModel.DataAnnotations;

namespace Havensread.Data.App;

public sealed class Genre
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public ICollection<Book> Books { get; } = [];
}

// note:
// a better implementation is to to let the book have many tags with m-m relation
// and a tag have many genres with m-m relation
// omitting due to mvp simplicity

//public sealed class Tag
//{
//    [Key]
//    public Guid Id { get; }

//    [Required]
//    public required string Name { get; set; }

//    public ICollection<Book> Books { get; } = [];


//}
