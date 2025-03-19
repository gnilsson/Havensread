namespace Havensread.Api.Endpoints;

public static class RoutingNames
{
    public static class Group
    {
        public const string Books = "books";
        public const string Authors = "authors";
    }

    public static class Endpoint
    {
        public const string GetBooks = nameof(GetBooks);
    }
}
