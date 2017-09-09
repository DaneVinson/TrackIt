using System;
using System.Collections.Generic;
using System.Text;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Domain.Model
{
    public static class ModelUtility
    {
        public static string GetPluralizedModelName<T>()
        {
            var type = typeof(T);
            if (type == typeof(Category)) { return "Categories"; }
            else if (type == typeof(DataPoint)) { return "DataPoints"; }
            else { throw new Exception($"Missing pluralization for type {type.Name}"); }
        }
    }
}
