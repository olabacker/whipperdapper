using System.Reflection;
using Dapper.Contrib.Extensions;

namespace WhipperDapper;

internal static class TypeExtensions
{
    public static string GetTableName(this Type type)
    {
        // The table attribute can exist in different namespaces ie System.ComponentModel.DataAnnotations and Dapper.Contrib so this covers it either way.
        var tableAttrName =
            type.GetCustomAttribute<TableAttribute>(false)?.Name
            ?? (type.GetCustomAttributes(false).FirstOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic)?.Name;

        if (tableAttrName != null)
        {
            return tableAttrName;
        }

        var name = type.Name + "s";
        if (type.IsInterface && name.StartsWith("I"))
        {
            name = name[1..];
        }

        return name;
    }
}