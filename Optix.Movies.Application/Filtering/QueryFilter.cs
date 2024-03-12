using Optix.Movies.Infrastructure.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace Optix.Movies.Application.Filtering
{
    public interface IQueryFilter<T>
    {
        int PageSize { get; set; }
        int Page { get; set; }
        SortDirection SortDirection { get; set; }
        string OrderBy { get; set; }

        Expression<Func<T, bool>> Filter { get; set; }
        Expression<Func<T, object>> OrderExpression { get; set; }
        Expression<Func<T, bool>> NotNullExpression { get; set; }

        void Create(string queryString);
    }
    public class QueryFilter<T> : IQueryFilter<T> where T : class
    {
        public int PageSize { get; set; } = 1000;
        public int Page { get; set; } = 1;
        public SortDirection SortDirection { get; set; }
        public string OrderBy { get; set; }

        public Expression<Func<T, bool>> Filter { get; set; }
        public Expression<Func<T, object>> OrderExpression { get; set; }
        public Expression<Func<T, bool>> NotNullExpression { get; set; }

        private readonly IExpressionParser _expressionParser;

        public QueryFilter(IExpressionParser expressionParser)
        {
            _expressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        }

        public void Create(string queryString)
        {
            // TODO: add validation

            Type itemType = typeof(T);
            var parameter = Expression.Parameter(itemType, "x");
            var filterAndValues = queryString.Split('&').ToList();
            ExtractPagination(filterAndValues);
            ExtractOrder(filterAndValues, itemType);
            ExtractFilters(filterAndValues, parameter, itemType);
        }

        private void ExtractPagination(List<string> filter)
        {
            var page = filter.Find(x => x.Contains("page", StringComparison.InvariantCultureIgnoreCase));
            var pageSize = filter.Find(x => x.Contains("pagesize", StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(page))
            {
                Page = int.Parse(page.Remove(0, 5));
            }

            if (!string.IsNullOrWhiteSpace(pageSize))
            {
                PageSize = int.Parse(pageSize.Remove(0, 9));
            }
        }

        private void ExtractOrder(List<string> filter, Type itemType)
        {
            var order = filter.Find(x => x.Contains("order", StringComparison.InvariantCultureIgnoreCase));
            var orderByProperty = filter.Find(x => x.Contains("orderBy", StringComparison.InvariantCultureIgnoreCase));

            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderItems = order.Split('=');
                if (orderItems.Length == 2)
                {
                    SortDirection = orderItems[1].Equals("Asc", StringComparison.InvariantCultureIgnoreCase) ? SortDirection.Ascending : SortDirection.Descending;
                }
                else
                {
                    SortDirection = SortDirection.Descending;
                }
            }

            if (!string.IsNullOrWhiteSpace(orderByProperty))
            {
                var orderProperties = orderByProperty.Split('=');
                if (orderProperties.Length == 2)
                {
                    OrderBy = orderProperties[1];
                }
                else
                {
                    OrderBy = "Name";
                }
            }

            if (!string.IsNullOrWhiteSpace(order) && string.IsNullOrWhiteSpace(orderByProperty))
            {
                OrderBy = "Name";
            }

            if (OrderBy != null)
            {
                PropertyInfo property = itemType.GetProperty(OrderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                var parameter = Expression.Parameter(itemType, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                OrderExpression = (Expression<Func<T, object>>)Expression.Lambda(Expression.Convert(propertyAccess, typeof(Object)).Reduce(), parameter);
            }
        }

        private void ExtractFilters(List<string> filter, ParameterExpression parameter, Type itemType)
        {
            var excluded = new List<string>() { "order", "page", "pagesize", "orderBy" };
            var filterAndValues = filter.Where(x =>
                        !excluded.Exists(e => x.Contains(e, StringComparison.InvariantCultureIgnoreCase))
                        ).ToList();

            LambdaExpression finalExpression = null;

            AddNotNullExpressions(filterAndValues, parameter, itemType);
            var currentExpression = GetAndExpression(filterAndValues, parameter, itemType);
            var currentOrExpression = GetOrExpression(filterAndValues, parameter, itemType);

            if (currentOrExpression != null && currentExpression != null)
            {
                currentExpression = Expression.And(currentExpression, currentOrExpression);
            }

            if (currentOrExpression != null && currentExpression == null)
            {
                currentExpression = currentOrExpression;
            }

            if (currentExpression != null)
            {
                finalExpression = Expression.Lambda<Func<T, bool>>(currentExpression, parameter);

                Filter = (Expression<Func<T, bool>>)finalExpression;
            }
        }

        private Expression GetAndExpression(List<string> filterAndValues, ParameterExpression parameter, Type itemType)
        {
            Expression currentExpression = null;
            foreach (var fv in filterAndValues.Where(x => !x.Contains("||")))
            {


                Expression expression = _expressionParser.GetExpression(parameter, itemType, fv);

                if (currentExpression == null)
                {
                    currentExpression = expression;
                }
                else
                {
                    currentExpression = Expression.And(currentExpression, expression);
                }
            }
            return currentExpression;
        }

        private Expression GetOrExpression(List<string> filterAndValues, ParameterExpression parameter, Type itemType)
        {
            Expression currentOrExpression = null;
            foreach (var fvo in filterAndValues.Where(x => x.Contains("||")))
            {

                var values = fvo.Split("||");

                foreach (var v in values)
                {
                    Expression orExpression = _expressionParser.GetExpression(parameter, itemType, v);

                    if (currentOrExpression == null)
                    {
                        currentOrExpression = orExpression;
                    }
                    else
                    {
                        currentOrExpression = Expression.Or(currentOrExpression, orExpression);
                    }
                }
            }
            return currentOrExpression;
        }

        private void AddNotNullExpressions(List<string> filterAndValues, ParameterExpression parameter, Type itemType)
        {
            Expression currentExpression = null;
            foreach (var fv in filterAndValues)
            {
                if (!fv.Contains('%'))
                {
                    continue;
                }

                var (_, property) = _expressionParser.DefineOperation(fv, itemType);
                var notNullExpression = Expression.NotEqual(Expression.Property(parameter, property), Expression.Constant(null));
                if (currentExpression == null)
                {
                    currentExpression = notNullExpression;
                }
                else
                {
                    currentExpression = Expression.And(currentExpression, notNullExpression);
                }


            }
            if (currentExpression != null)
            {
                var finalExpression = Expression.Lambda<Func<T, bool>>(currentExpression, parameter);

                NotNullExpression = finalExpression;
            }
        }
    }
}
