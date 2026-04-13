using System.Linq.Expressions;
using IntegrationBus.Core.Models;

namespace IntegrationBus.Core.Interfaces;

public interface IDynamicFilterService
{
    Expression<Func<Order, bool>> BuildFilter(Dictionary<string, string> filters);
}
