using System.Linq.Expressions;

namespace ModularMonolith.Shared.DataAccess.Models;

public class Paginator<T>
{
    private readonly Expression<Func<T, object>> _orderByExpression;
    private readonly bool _isAscending;

    public Paginator(Expression<Func<T, object>> orderByExpression, bool isAscending, long skip, long take)
    {
        _orderByExpression = orderByExpression;
        _isAscending = isAscending;
        Skip = skip;
        Take = take;
    }

    public long Skip { get; private set; }

    public long Take { get; private set; }

    public (Expression<Func<T, object>> Expression, bool IsAscending) GetOrderByExpression() => (_orderByExpression, _isAscending);

    public static Paginator<T> Ascending(long skip, long take, Expression<Func<T, object>> orderByExpression) =>
        new(orderByExpression, true, skip, take);

    public static Paginator<T> Descending(long skip, long take, Expression<Func<T, object>> orderByExpression) =>
        new(orderByExpression, false, skip, take);

}
