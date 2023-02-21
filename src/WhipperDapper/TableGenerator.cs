using System.Reflection;

namespace WhipperDapper;

public class TableGenerator
{
    public string GenerateCreateQuery<T>()
    {
        var type = typeof(T);
        var tableName = type.GetTableName();

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var columns = new string[properties.Length];

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var columnName = property.Name.ToLower();
            var columnType = GetColumnType(property.PropertyType);

            columns[i] = $"{columnName} {columnType}";
        }

        var createTable = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)} PRIMARY KEY (`Id`) USING BTREE);";
        return createTable;
    }

    private static string GetColumnType(Type type)
        => type switch
        {
            _ when type == typeof(int?) => "INT(11) NULL",
            _ when type == typeof(int) => "INT(11)",
            _ when type == typeof(string) => "VARCHAR(255)",
            _ when type == typeof(DateTime) => "DATETIME",
            _ => throw new ArgumentException($"Unsupported data type {type.Name}")
        };
}