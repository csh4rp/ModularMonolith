namespace ModularMonolith.Shared.Contracts;

public interface IPaginatedQuery
{
    string? OrderBy { get; }

    int? Skip { get; }

    int? Take { get; }
}
