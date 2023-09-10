using System.Reflection;

namespace WhipperDapper;

public class TableGenerator
{
    public static string GenerateCreateQuery<T>()
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
            
            if(columnName == "id")
            {
                columnType = "INT(11) NOT NULL AUTO_INCREMENT";
            }

            columns[i] = $"`{columnName}` {columnType}";
        }

        var createTable = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)}, PRIMARY KEY (`id`) USING BTREE);";
        return createTable;
    }

    private static string GetColumnType(Type type)
        => type switch
        {
            _ when type == typeof(int?) => "INT(11) NULL",
            _ when type == typeof(int) => "INT(11)",
            _ when type == typeof(long?) => "BIGINT(20) NULL",
            _ when type == typeof(long) => "BIGINT(20)",
            _ when type == typeof(string) => "VARCHAR(255)",
            _ when type == typeof(bool?) => "TINYINT(1) NULL",
            _ when type == typeof(bool) => "TINYINT(1)",
            _ when type == typeof(double?) => "DOUBLE NULL",
            _ when type == typeof(double) => "DOUBLE",
            _ when type == typeof(float?) => "FLOAT NULL",
            _ when type == typeof(float) => "FLOAT",
            _ when type == typeof(decimal?) => "DECIMAL NULL",
            _ when type == typeof(decimal) => "DECIMAL",
            _ when type == typeof(DateTime?) => "DATETIME NULL",
            _ when type == typeof(DateTime) => "DATETIME",
            _ when type == typeof(byte) => "TINYINT(1)",
            _ when type == typeof(byte?) => "TINYINT(1) NULL",
            _ when type.IsEnum => "INT(11)",
            _ => throw new ArgumentException($"Unsupported data type {type.Name}")
        };
}