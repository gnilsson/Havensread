using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;

namespace Havensread.DataIngestor;

public sealed class Kaggle
{
    public sealed class Settings
    {
        public const string SectionName = "Kaggle";
        public required string Username { get; init; }
        public required string ApiKey { get; init; }
    }

    public sealed class BooksIngestion
    {
        public required IEnumerable<Book> Books { get; init; }
    }

    public sealed class Book
    {
        public required int BookID { get; init; }
        public required string Title { get; init; }
        public required string Authors { get; init; }
        public required float AverageRating { get; init; }
        public required int RatingsCount { get; init; }
        public required int TextReviewsCount { get; init; }
        public string? ISBN { get; init; }
        public string? ISBN13 { get; init; }
        public string? LanguageCode { get; init; }
        public int? NumPages { get; init; }
        public string? PublicationDate { get; init; }
        public string? Publisher { get; init; }
    }


    public sealed class BookMap : ClassMap<Book>
    {
        private sealed class FloatConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
            {
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }

                Console.WriteLine($"Invalid float value '{text}' at row nr {row.Context.Writer?.Row}");
                return 0f;
            }
        }

        public BookMap()
        {
            Map(m => m.BookID).Name("bookID");
            Map(m => m.Title).Name("title");
            Map(m => m.Authors).Name("authors");
            Map(m => m.AverageRating).Name("average_rating").TypeConverter<FloatConverter>().Default(0);
            Map(m => m.RatingsCount).Name("ratings_count").Default(0);
            Map(m => m.TextReviewsCount).Name("text_reviews_count").Default(0);
            Map(m => m.ISBN).Name("isbn").Optional();
            Map(m => m.ISBN13).Name("isbn13").Optional();
            Map(m => m.LanguageCode).Name("language_code").Optional();
            Map(m => m.NumPages).Name("num_pages").Optional();
            Map(m => m.PublicationDate).Name("publication_date").Optional();
            Map(m => m.Publisher).Name("publisher").Optional();
        }
    }
}
