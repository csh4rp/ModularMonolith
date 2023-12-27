using System.Linq.Expressions;
using System.Reflection;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Contracts.Attributes;

namespace ModularMonolith.Shared.Infrastructure.DataAccess.Extensions;

public static class DataAccessExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, IPaginatedQuery paginator)
    {
        IOrderedQueryable<T> query;

        var objType = typeof(T);

        var (fieldName, isAscending) = GetOrderInfo(paginator.OrderBy);

        if (!string.IsNullOrEmpty(fieldName) && isAscending.HasValue)
        {
            var property = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Single(p => fieldName.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase));
            
            var method = typeof(DataAccessExtensions).GetMethod(nameof(ApplyOrdering), BindingFlags.Static | BindingFlags.NonPublic)!;
            var genericMethod = method.MakeGenericMethod(objType, property.PropertyType);

            query = (IOrderedQueryable<T>)genericMethod.Invoke(null,
                [queryable, property, isAscending.Value])!;
        }
        else
        {
            var attribute = paginator.GetType()
                .GetProperty(nameof(paginator.OrderBy))!
                .GetCustomAttribute<DefaultOrderByAttribute>();

            var property = attribute is null
                ? objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)[0]
                : objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Single(p => p.Name.Equals(attribute.PropertyName, StringComparison.InvariantCultureIgnoreCase));

            isAscending = attribute?.IsAscending ?? true;

            if (attribute is null)
            {
                var method = typeof(DataAccessExtensions).GetMethod(nameof(ApplyOrdering), BindingFlags.Static | BindingFlags.NonPublic)!;
                var genericMethod = method.MakeGenericMethod(objType, property.PropertyType);

                query = (IOrderedQueryable<T>)genericMethod.Invoke(null, [queryable, property, isAscending])!;
            }
            else
            {
                var method = typeof(DataAccessExtensions).GetMethod(nameof(ApplyOrdering), BindingFlags.Static | BindingFlags.NonPublic)!;
                var genericMethod = method.MakeGenericMethod(objType, property.PropertyType);

                query = (IOrderedQueryable<T>)genericMethod.Invoke(null, [queryable, property, isAscending])!;
            }
        }

        queryable = paginator.Skip.HasValue ? query.Skip(paginator.Skip.Value) : query;

        if (paginator.Take.HasValue)
        {
            queryable = queryable.Take(paginator.Take.Value);
        }
        else
        {
            var attribute = paginator.GetType()
                .GetProperty(nameof(paginator.Take))!
                .GetCustomAttribute<DefaultTakeAttribute>();

            if (attribute is not null)
            {
                queryable = queryable.Take(attribute.Take);
            }
        }

        return queryable;
    }

    private static (string? FieldName, bool? IsDescending) GetOrderInfo(string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            return default;
        }

        var index = orderBy.IndexOf(':');

        if (index == -1)
        {
            index = orderBy.Length - 1;
        }

        var span = orderBy.AsSpan();

        var fieldName = span[..index];
        var direction = span[(index + 1)..].ToString();

        var isAscending = !direction.Equals("desc", StringComparison.InvariantCultureIgnoreCase);

        return (fieldName.ToString(), isAscending);
    }

    private static IOrderedQueryable<T> ApplyOrdering<T, TK>(IQueryable<T> queryable,
        PropertyInfo propertyInfo,
        bool isAscending)
    {
        var objType = propertyInfo.DeclaringType;
        var parameterExpression = Expression.Parameter(objType!, "x");
        var propertyExpression = Expression.Property(parameterExpression, propertyInfo);
        var expression = Expression.Lambda<Func<T, TK>>(propertyExpression, parameterExpression);

        return isAscending
            ? queryable.OrderBy(expression)
            : queryable.OrderByDescending(expression);
    }
}
