using Microsoft.AspNetCore.Identity;

namespace NIC_clinica_project.Repositories
{
    public abstract class SortableRepository
    {
        private readonly IReadOnlyDictionary<string, string> _orderByValues;

        protected SortableRepository(IReadOnlyDictionary<string, string> orderByValues)
        {
            _orderByValues = orderByValues;
        }

        public bool IsSortable(string? orderParam)
        {
            return !string.IsNullOrWhiteSpace(orderParam) && _orderByValues.ContainsKey(orderParam);
        }
        
        protected string GetOrderByValue(string orderParam)
        {
            if (string.IsNullOrWhiteSpace(orderParam) || !_orderByValues.TryGetValue(orderParam, out var value))
            {
                throw new ArgumentException("Недопустимое поле сортировки.", nameof(orderParam));
            }
            return value;
        }
    }
}
