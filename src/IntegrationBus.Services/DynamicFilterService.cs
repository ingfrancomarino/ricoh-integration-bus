using System.Linq.Expressions;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Core.Models;

namespace IntegrationBus.Services;

public class DynamicFilterService : IDynamicFilterService
{
    public Expression<Func<Order, bool>> BuildFilter(Dictionary<string, string> filters)
    {
        var parameter = Expression.Parameter(typeof(Order), "o");
        Expression? combined = null;

        foreach (var (key, value) in filters)
        {
            Expression comparison = key switch
            {
                "Status" => Expression.Equal(
                    Expression.Property(parameter, nameof(Order.Status)),
                    Expression.Constant(value)),

                "Product" => Expression.Equal(
                    Expression.Property(parameter, nameof(Order.Product)),
                    Expression.Constant(value)),

                "MinAmount" => Expression.GreaterThanOrEqual(
                    Expression.Property(parameter, nameof(Order.Amount)),
                    Expression.Constant(decimal.Parse(value))),

                "MaxAmount" => Expression.LessThanOrEqual(
                    Expression.Property(parameter, nameof(Order.Amount)),
                    Expression.Constant(decimal.Parse(value))),

                _ => throw new ArgumentException($"Unknown filter key: '{key}'.", nameof(filters))
            };

            combined = combined is null
                ? comparison
                : Expression.AndAlso(combined, comparison);
        }

        combined ??= Expression.Constant(true);
        return Expression.Lambda<Func<Order, bool>>(combined, parameter);
    }
}
