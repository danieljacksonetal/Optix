using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Optix.Movies.Application.Filtering
{
    public interface IExpressionParser
    {
        List<PropertyInfo> Properties { get; set; }
        object Value { get; set; }
        void DefineValue(string filterValues, Type itemType);
        Expression GetExpression(ParameterExpression parameter, Type itemType, string filterAndValue);
        (string[], PropertyInfo) DefineOperation(string filterValues, Type itemType);

    }
    public class ExpressionParser : IExpressionParser
    {

        public List<PropertyInfo> Properties { get; set; }
        public object Value { get; set; }
        public Operator Condition { get; set; }

        //for testing
        public ExpressionParser(string filterValues, Type itemType)
        {
            DefineValue(filterValues, itemType);
        }

        //for testing
        public ExpressionParser()
        {

        }

        // so we can use DI
        public void DefineValue(string filterValues, Type itemType)
        {
            Properties = new List<PropertyInfo>();
            Value = new object();
            var (values, _) = DefineOperation(filterValues, itemType);
            if (values.Any())
            {
                Value = ParseValue(values[1]);
            }
        }

        public Expression GetExpression(ParameterExpression parameter, Type itemType, string filterAndValue)
        {
            DefineValue(filterAndValue, itemType);
            if (Value != null)
            {
                var expression = GetExpression(parameter);
                return expression;
            }
            return null;
        }

        private Expression GetExpression(ParameterExpression parameter)
        {
            var constantExpression = Expression.Constant(Value);
            Expression returnExpression;

            if (!Properties.Any())
            {
                return null;
            }

            //making constant nullable
            if (Nullable.GetUnderlyingType(Properties[Properties.Count - 1].PropertyType) != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(Properties[Properties.Count - 1].PropertyType));
                constantExpression = Expression.Constant(Value, type);
            }

            Expression body = parameter;
            foreach (var member in Properties)
            {
                body = Expression.Property(body, member);
            }

            switch (Condition)
            {
                default:
                case Operator.Equals:
                    {
                        returnExpression = Expression.Equal(body, constantExpression);
                        break;
                    }
                case Operator.Contains:
                    {
                        constantExpression = Expression.Constant(Value.ToString().ToLower());

                        MethodInfo toLowerMethod = typeof(string).GetMethod("ToLowerInvariant");

                        var expression1 = Expression.Call(body, toLowerMethod);

                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                        returnExpression = Expression.Call(expression1, method, constantExpression);

                        break;

                    }
                case Operator.ContainsCaseSensitive:
                    {
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        returnExpression = Expression.Call(body, method, constantExpression);

                        break;
                    }
                case Operator.GreaterThan:
                    {
                        returnExpression = Expression.GreaterThan(body, constantExpression);

                        break;
                    }
                case Operator.LessThan:
                    {
                        returnExpression = Expression.LessThan(body, constantExpression);
                        break;
                    }
                case Operator.GreaterOrEqual:
                    {
                        returnExpression = Expression.GreaterThanOrEqual(body, constantExpression);
                        break;
                    }
                case Operator.LessOrEqual:
                    {
                        returnExpression = Expression.LessThanOrEqual(body, constantExpression);
                        break;
                    }
                case Operator.NotEquals:
                    {
                        returnExpression = Expression.NotEqual(body, constantExpression);
                        break;
                    }
            }

            return returnExpression;
        }


        public (string[], PropertyInfo) DefineOperation(string filterValues, Type itemType)
        {
            string[] values = null;

            if (filterValues.Contains('='))
            {
                values = filterValues.Split('=');
                Condition = Operator.Equals;
            }

            if (filterValues.Contains('%'))
            {
                if (filterValues.Contains("%%"))
                {
                    Condition = Operator.ContainsCaseSensitive;
                    values = Regex.Split(filterValues, "%%");
                }
                else
                {
                    Condition = Operator.Contains;
                    values = filterValues.Split('%');
                }
            }

            if (filterValues.Contains('>'))
            {
                values = filterValues.Split('>');
                Condition = Operator.GreaterThan;
            }

            if (filterValues.Contains('<'))
            {
                values = filterValues.Split('<');
                Condition = Operator.LessThan;
            }

            if (filterValues.Contains(">="))
            {
                values = Regex.Split(filterValues, ">=");
                Condition = Operator.GreaterOrEqual;
            }

            if (filterValues.Contains("<="))
            {
                values = Regex.Split(filterValues, "<=");
                Condition = Operator.LessOrEqual;
            }

            if (filterValues.Contains("!="))
            {
                values = Regex.Split(filterValues, "!=");
                Condition = Operator.NotEquals;
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(filterValues));
            }

            var property = itemType.GetProperty(values[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

            if (property != null)
            {
                Properties?.Add(property);
            }

            return (values, property);
        }

        public object ParseValue(string value)
        {
            object parsedValue = null;

            foreach (var property in Properties.Select(property => property.PropertyType))
            {
                if (property.IsClass && property.Name.ToLower() != "string" && property.Name.ToLower() != "datetime")
                {
                    continue;
                }
                //Verifying if is nullable
                if (Nullable.GetUnderlyingType(property) != null)
                {
                    var underlyingType = Nullable.GetUnderlyingType(property);
                    var type = typeof(Nullable<>).MakeGenericType(underlyingType);

                    var newValue = underlyingType.IsEnum ?
                            Enum.Parse(underlyingType, value) :
                            ChangeType(value, underlyingType);

                    var nullableObject = Activator.CreateInstance(type, newValue);

                    parsedValue = nullableObject;
                }
                else
                {
                    parsedValue = ChangeType(value, property);
                }
            }

            return parsedValue;
        }

        private static object ChangeType(string value, Type type)
        {
            if (type.IsEnum)
            {
                return Convert.ChangeType(Enum.Parse(type, value), type);
            }

            if (type == typeof(Guid))
            {
                return Guid.Parse(value);
            }

            if (type == typeof(BigInteger))
            {
                return BigInteger.Parse(value);
            }

            if (type == typeof(int))
            {
                return int.Parse(value);
            }

            if (type == typeof(DateTime))
            {
                return DateTime.Parse(value);
            }

            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFrom(value);
        }
    }

    public enum Operator
    {
        Equals = 1,
        Contains = 2,
        ContainsCaseSensitive = 3,
        GreaterThan = 4,
        LessThan = 5,
        GreaterOrEqual = 6,
        LessOrEqual = 7,
        NotEquals = 8

    }
}
