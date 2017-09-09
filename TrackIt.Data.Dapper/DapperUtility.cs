using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.Dapper
{
    internal static class DapperUtility
    {
        internal static string GetPropertiesAssignmentSqlString<T>()
        {
            var type = typeof(T);
            var stringBuilder = new StringBuilder();
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != "Id")
                .ToList()
                .ForEach(p =>
                {
                    stringBuilder.Append($"{p.Name} = @{p.Name}, ");
                });
            return stringBuilder.ToString().TrimEnd().TrimEnd(',');
        }

        internal static string GetPropertyNames<T>(bool parameterize = false)
        {
            var type = typeof(T);
            var paramMarker = parameterize ? "@" : "";
            var stringBuilder = new StringBuilder();
            return String.Join(
                            ",",
                            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => !"DataPoints".Equals(p.Name))
                                .Select(p => String.Concat(paramMarker, p.Name))
                                .ToArray());
        }
    }
}
