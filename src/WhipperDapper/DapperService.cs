using Dapper.Contrib.Extensions;
using MySqlConnector;
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace WhipperDapper;

public class DapperService
{
    private readonly DapperSettings _dapperSettings;

    public DapperService(DapperSettings dapperSettings) => _dapperSettings = dapperSettings;

    private async Task<T> UseConnection<T>(Func<IDbConnection, Task<T>> action)
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);
        await connection.OpenAsync();
        return await action(connection);
    }

    public async Task Query(Action<IDbConnection> action)
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();
        action(connection);
    }

    /// <summary>
    /// Returns all entities of type T
    /// If func is not null it will return all entities that match the predicate
    /// The expression => query builder is pretty bad and only supports basic expressions
    /// </summary>
    /// <param name="func">Empty returns all</param>
    /// <param name="skip">Number of entities to skip</param>
    /// <param name="limit">Maximum number of results</param>
    /// <typeparam name="T">Type of entity</typeparam>
    /// <returns></returns>
    public Task<IEnumerable<T>> Query<T>(Expression<Func<T, bool>>? func = null, int? skip = null, int? limit = null) where T : class, IEntity
    {
        if (func is null)
        {
            return UseConnection(c => c.GetAllAsync<T>());
        }

        var translator = new PredicateQueryTranslator();
        var whereClause = translator.Translate(func);
        var table = typeof(T).GetTableName();
        var query = $"SELECT * FROM {table} WHERE {whereClause}";

        if (skip.HasValue)
        {
            query += $" OFFSET {skip.Value}";
        }
        
        if (limit.HasValue)
        {
            query += $" LIMIT {limit.Value}";
        }
        
        return UseConnection(c => c.QueryAsync<T>(query));
    }

    public async Task<T> QuerySingle<T>(Expression<Func<T, bool>> func) where T : class, IEntity
    {
        var translator = new PredicateQueryTranslator();
        var whereClause = translator.Translate(func);
        var table = typeof(T).GetTableName();
        var query = $"SELECT * FROM {table} WHERE {whereClause} LIMIT 1";
        var result = await UseConnection(c => c.QueryAsync<T>(query));
        return result.First();
    }

    public Task<T> Get<T>(int id) where T : class =>
        UseConnection(c => c.GetAsync<T>(id));

    public Task Save<T>(T entity) where T : class, IEntity
    {
        if (entity.Id == 0)
        {
            return UseConnection(c => c.InsertAsync(entity));
        }

        return UseConnection(c => c.UpdateAsync(entity));
    }

    public Task<bool> Delete<T>(T entity) where T : class, IEntity => 
        UseConnection(c => c.DeleteAsync(entity));
}