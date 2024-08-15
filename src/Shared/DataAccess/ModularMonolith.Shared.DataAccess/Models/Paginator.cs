namespace ModularMonolith.Shared.DataAccess.Models;

public sealed record Paginator
{
    private static readonly Paginator Default = new(0, int.MaxValue, true);

    private Paginator(int skip, int take, bool isAscending)
    {
        Skip = skip;
        Take = take;
        IsAscending = isAscending;
    }

    public int Skip { get; private set; }

    public int Take { get; private set; }

    public bool IsAscending { get; private set; }

    public static Paginator Ascending(int skip, int take) => new(skip, take, true);

    public static Paginator Descending(int skip, int take) => new(skip, take, false);
}
