using System.ComponentModel.DataAnnotations;

namespace Havensread.Api;

public static class GoogleSearch
{
    public class Result
    {
        public required string Kind { get; set; }

        public required Url Url { get; set; }

        public required Queries Queries { get; set; }

        public Context? Context { get; set; }

        public required SearchInformation SearchInformation { get; set; }

        public List<Item>? Items { get; set; }
    }

    public class Url
    {
        public required string Type { get; set; }

        public required string Template { get; set; }
    }

    public class Queries
    {
        public List<NextPage>? NextPage { get; set; }

        public required List<Request> Request { get; set; }
    }

    public class NextPage
    {
        public string? Title { get; set; }

        public string? TotalResults { get; set; }

        public required string SearchTerms { get; set; }

        public int Count { get; set; }

        public int StartIndex { get; set; }

        public string? InputEncoding { get; set; }

        public string? OutputEncoding { get; set; }

        public string? Safe { get; set; }

        public required string Cx { get; set; }
    }

    public class Request
    {
        public string? Title { get; set; }

        public string? TotalResults { get; set; }

        public required string SearchTerms { get; set; }

        public int Count { get; set; }

        public int StartIndex { get; set; }

        public string? InputEncoding { get; set; }

        public string? OutputEncoding { get; set; }

        public string? Safe { get; set; }

        public required string Cx { get; set; }
    }

    public class Context
    {
        public string? Title { get; set; }
    }

    public class SearchInformation
    {
        public double SearchTime { get; set; }

        public string? FormattedSearchTime { get; set; }

        public required string TotalResults { get; set; }

        public string? FormattedTotalResults { get; set; }
    }

    public class Item
    {
        public required string Kind { get; set; }

        public required string Title { get; set; }

        public string? HtmlTitle { get; set; }

        public required string Link { get; set; }

        public string? DisplayLink { get; set; }

        public string? Snippet { get; set; }

        public string? HtmlSnippet { get; set; }

        public string? FormattedUrl { get; set; }

        public string? HtmlFormattedUrl { get; set; }

        public Pagemap? Pagemap { get; set; }
    }

    public class Pagemap
    {
        public List<CseThumbnail>? CseThumbnail { get; set; }

        public List<Metatag>? Metatags { get; set; }

        public List<CseImage>? CseImage { get; set; }
    }

    public class CseThumbnail
    {
        [Required]
        public required string Src { get; set; }

        public string? Width { get; set; }

        public string? Height { get; set; }
    }

    public class Metatag
    {
        public string? Viewport { get; set; }

        public string? Author { get; set; }

        public string? OgTitle { get; set; }

        public string? OgDescription { get; set; }

        public string? OgUrl { get; set; }

        public string? OgSiteName { get; set; }
    }

    public class CseImage
    {
        [Required]
        public required string Src { get; set; }
    }
}
