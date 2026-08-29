using System.Linq.Expressions;
using System.Reflection;

namespace Pars.Application.Common.Filtering;

/// <summary>
/// سازنده کوئری پویا با Expression Trees
/// </summary>
public static class QueryBuilder<T> where T : class
{
    public static IQueryable<T> Apply(
        IQueryable<T> query,
        QueryRequest request,
        Expression<Func<T, bool>>? globalSearchFilter = null)
    {
        // 1. اعمال فیلترهای عمومی
        if (!string.IsNullOrWhiteSpace(request.GlobalSearch) && globalSearchFilter != null)
        {
            query = query.Where(globalSearchFilter);
        }

        // 2. اعمال فیلترهای اختصاصی
        foreach (var filter in request.Filters)
        {
            query = ApplyFilter(query, filter);
        }

        // 3. اعمال مرتب‌سازی
        query = ApplySorting(query, request.Sorts);

        // 4. اعمال صفحه‌بندی
        query = ApplyPaging(query, request.Page, request.PageSize);

        return query;
    }

    private static IQueryable<T> ApplyFilter(IQueryable<T> query, FilterDescriptor filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(filter.Field,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return query;

        var member = Expression.Property(parameter, property);
        Expression? comparison = null;

        switch (filter.Operator)
        {
            case FilterOperator.Equals:
                comparison = BuildComparison(Expression.Equal, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.NotEquals:
                comparison = BuildComparison(Expression.NotEqual, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.Contains:
                comparison = BuildStringMethod(member, filter.Value, "Contains");
                break;

            case FilterOperator.StartsWith:
                comparison = BuildStringMethod(member, filter.Value, "StartsWith");
                break;

            case FilterOperator.EndsWith:
                comparison = BuildStringMethod(member, filter.Value, "EndsWith");
                break;

            case FilterOperator.GreaterThan:
                comparison = BuildComparison(Expression.GreaterThan, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.GreaterThanOrEqual:
                comparison = BuildComparison(Expression.GreaterThanOrEqual, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.LessThan:
                comparison = BuildComparison(Expression.LessThan, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.LessThanOrEqual:
                comparison = BuildComparison(Expression.LessThanOrEqual, member, filter.Value, property.PropertyType);
                break;

            case FilterOperator.IsNull:
                comparison = Expression.Equal(member, Expression.Constant(null, property.PropertyType));
                break;

            case FilterOperator.IsNotNull:
                comparison = Expression.NotEqual(member, Expression.Constant(null, property.PropertyType));
                break;

            case FilterOperator.In:
                comparison = BuildInExpression(member, filter.Value, property.PropertyType);
                break;
        }

        if (comparison == null)
            return query;

        var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);

        return filter.Logic == FilterLogic.And
            ? query.Where(lambda)
            : query.Where(lambda); // OR needs different handling with Union
    }

    private static Expression BuildComparison(
        Func<Expression, Expression, BinaryExpression> comparison,
        MemberExpression member,
        object? value,
        Type targetType)
    {
        var converted = ConvertValue(value, targetType);
        var constant = Expression.Constant(converted, targetType);
        return comparison(member, constant);
    }

    private static Expression BuildStringMethod(
        MemberExpression member,
        object? value,
        string methodName)
    {
        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) });
        var constant = Expression.Constant(value?.ToString() ?? "");

        // Handle nullable string
        Expression target = member;
        if (Nullable.GetUnderlyingType(member.Type) != null)
            target = Expression.Property(member, "Value");

        return Expression.Call(target, method!, constant);
    }

    private static Expression BuildInExpression(
        MemberExpression member,
        object? value,
        Type targetType)
    {
        if (value is not System.Collections.IEnumerable values)
            return Expression.Constant(true);

        var listType = typeof(List<>).MakeGenericType(targetType);
        var typedList = Activator.CreateInstance(listType) as System.Collections.IList;

        foreach (var item in values)
        {
            typedList!.Add(ConvertValue(item, targetType));
        }

        var containsMethod = listType.GetMethod("Contains")!;
        var listConstant = Expression.Constant(typedList, listType);

        return Expression.Call(listConstant, containsMethod, member);
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null) return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlying == typeof(string))
                return value.ToString();

            if (underlying == typeof(int))
                return Convert.ToInt32(value);

            if (underlying == typeof(long))
                return Convert.ToInt64(value);

            if (underlying == typeof(decimal))
                return Convert.ToDecimal(value);

            if (underlying == typeof(double))
                return Convert.ToDouble(value);

            if (underlying == typeof(float))
                return Convert.ToSingle(value);

            if (underlying == typeof(bool))
                return Convert.ToBoolean(value);

            if (underlying == typeof(DateTime))
                return Convert.ToDateTime(value);

            if (underlying == typeof(DateOnly))
            {
                if (value is DateTime dt)
                    return DateOnly.FromDateTime(dt);
                if (DateTime.TryParse(value.ToString(), out var parsed))
                    return DateOnly.FromDateTime(parsed);
            }

            return Convert.ChangeType(value, underlying);
        }
        catch
        {
            return null;
        }
    }

    private static IQueryable<T> ApplySorting(IQueryable<T> query, List<SortDescriptor> sorts)
    {
        if (sorts.Count == 0)
            return query;

        IOrderedQueryable<T>? ordered = null;

        for (int i = 0; i < sorts.Count; i++)
        {
            var sort = sorts[i];
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(sort.Field,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null) continue;

            var member = Expression.Property(parameter, property);
            var lambda = Expression.Lambda<Func<T, object>>(
                Expression.Convert(member, typeof(object)), parameter);

            if (i == 0)
            {
                ordered = sort.Descending
                    ? query.OrderByDescending(lambda)
                    : query.OrderBy(lambda);
            }
            else
            {
                ordered = sort.Descending
                    ? ordered!.ThenByDescending(lambda)
                    : ordered!.ThenBy(lambda);
            }
        }

        return ordered ?? query;
    }

    private static IQueryable<T> ApplyPaging(IQueryable<T> query, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}