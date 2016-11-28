using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kirkin.DataAnalysis
{
    public sealed class PropertyValueSet
    {
        public PropertyInfo Property { get; set; }
        public ValueCount[] Values { get; set; }

        public override string ToString()
        {
            Type propertyType = Nullable.GetUnderlyingType(Property.PropertyType) ?? Property.PropertyType;

            return $"{Property.Name} ({propertyType.Name})";
        }
    }

    public sealed class ValueCount
    {
        public object Value { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return $"Value = {Value}; Count = {Count}";
        }
    }

    public abstract class DataInsight
    {
        public PropertyValueSet Stat { get; set; }
        public bool Nullable { get; set; }

        public override string ToString()
        {
            return $"{Stat}, {(Nullable ? "nullable" : "non-nullable")}, {Stat.Values.Length} distinct values";
        }
    }

    public sealed class IntegerInsight : DataInsight
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, from {MinValue} to {MaxValue}";
        }
    }

    public sealed class EnumInsight : DataInsight
    {
        public string[] DistinctValues { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}: " + string.Join(", ", DistinctValues.Select(v => v == null ? "NULL" : v));
        }
    }

    public sealed class StringInsight : DataInsight
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {MinLength} to {MaxLength} characters long";
        }
    }

    public sealed class NoInsight : DataInsight
    {
    }

    public interface IDataAnalyzer
    {
        DataInsight GenerateInsights(PropertyValueSet stat);
    }

    public sealed class DynamicInsightFactory : IDataAnalyzer
    {
        public DataInsight GenerateInsights(PropertyValueSet stat)
        {
            IDataAnalyzer analyzer = ResolveAnalyzer(stat);

            return analyzer == null ? new NoInsight { Stat = stat } : analyzer.GenerateInsights(stat);
        }

        private static IDataAnalyzer ResolveAnalyzer(PropertyValueSet stat)
        {
            if (stat.Values.Length <= 10) {
                return new EnumAnalyzer();
            }

            Type propertyType = stat.Property.PropertyType;

            if (typeof(int?).IsAssignableFrom(propertyType)) {
                return new IntegerAnalyzer();
            }

            if (propertyType == typeof(string)) {
                return new StringAnalyzer();
            }

            return null;
        }

        sealed class IntegerAnalyzer : IDataAnalyzer
        {
            public DataInsight GenerateInsights(PropertyValueSet stat)
            {
                return new IntegerInsight {
                    MaxValue = stat.Values.Select(v => v.Value).Cast<int?>().DefaultIfEmpty().Max() ?? 0,
                    MinValue = stat.Values.Select(v => v.Value).Cast<int?>().DefaultIfEmpty().Min() ?? 0,
                    Nullable = stat.Values.Any(v => v.Value == null),
                    Stat = stat
                };
            }
        }

        sealed class StringAnalyzer : IDataAnalyzer
        {
            public DataInsight GenerateInsights(PropertyValueSet stat)
            {
                return new StringInsight {
                    MaxLength = stat.Values.Select(v => v.Value).Cast<string>().Where(s => s != null).Select(s => s.Length).DefaultIfEmpty().Max(),
                    MinLength = stat.Values.Select(v => v.Value).Cast<string>().Where(s => s != null).Select(s => s.Length).DefaultIfEmpty().Min(),
                    Nullable = stat.Values.Any(v => v.Value == null),
                    Stat = stat
                };
            }
        }

        sealed class EnumAnalyzer : IDataAnalyzer
        {
            public DataInsight GenerateInsights(PropertyValueSet stat)
            {
                return new EnumInsight {
                    DistinctValues = stat.Values.Select(v => v.Value?.ToString()).ToArray(),
                    Nullable = stat.Values.Any(v => v.Value == null),
                    Stat = stat
                };
            }
        }
    }
}