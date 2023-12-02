using System.Linq.Expressions;
using System.Reflection;
using ModularMonolith.Shared.Contracts;

namespace ModularMonolith.Shared.Infrastructure.DataAccess;

public static class DataAccessExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, IPaginatedQuery paginator)
    {
        IOrderedQueryable<T> query;

        var objType = typeof(T);

        var (fieldName, isAscending) = GetOrderInfo(paginator.OrderBy);
        
        if (!string.IsNullOrEmpty(fieldName) && isAscending.HasValue)
        {
            var property = objType.GetProperty(fieldName)!;
            var method = typeof(DataAccessExtensions).GetMethod(nameof(ApplyOrdering))!;
            var genericMethod = method.MakeGenericMethod(objType, property.PropertyType);

            query = (IOrderedQueryable<T>) genericMethod.Invoke(null, new object[] { queryable, property, isAscending.Value })!;
        }
        else
        {
            var property = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance)[0];
            var method = typeof(DataAccessExtensions).GetMethod(nameof(ApplyOrdering))!;
            var genericMethod = method.MakeGenericMethod(objType, property.PropertyType);

            query = (IOrderedQueryable<T>) genericMethod.Invoke(null, new object[] { queryable, property, true })!;
        }
        
        if (paginator.Skip.HasValue)
        {
            queryable = query.Skip(paginator.Skip.Value);
        }

        if (paginator.Take.HasValue)
        {
            queryable = queryable.Take(paginator.Take.Value);
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
        var direction = span[index..];

        var isDescending = direction is "desc";

        return (fieldName.ToString(), isDescending);
    }
    
    private static IOrderedQueryable<T> ApplyOrdering<T, TK>(IQueryable<T> queryable, PropertyInfo propertyInfo, bool isAscending)
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
