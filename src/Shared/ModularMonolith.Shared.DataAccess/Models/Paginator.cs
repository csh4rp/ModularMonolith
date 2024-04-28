namespace ModularMonolith.Shared.DataAccess.Models;

public sealed record Paginator
{
    private static readonly Paginator Default = new(0, int.MaxValue, true);

    private Paginator(long skip, long take, bool isAscending)
    {
        Skip = skip;
        Take = take;
        IsAscending = isAscending;
    }

    public long Skip { get; private set; }

    public long Take { get; private set; }

    public bool IsAscending { get; private set; }

    public static Paginator Ascending(long skip, long take) => new(skip, take, true);

    public static Paginator Descending(long skip, long take) => new(skip, take, false);
}
