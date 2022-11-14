using Dapper.Contrib.Extensions;
using MySqlConnector;
using System.Data;

namespace WhipperDapper;
public class DapperService
{
    private readonly DapperSettings _dapperSettings;

    public DapperService(DapperSettings dapperSettings) => _dapperSettings = dapperSettings;

    public async Task Query(Action<IDbConnection> action)
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();
        action(connection);
    }

    public async Task<IEnumerable<T>> GetWhere<T>(Func<T, bool> func) where T : class, IEntity
    {
        var all = await GetAll<T>();
        return all.Where(func);
    }

    public async Task<T> GetFirstWhere<T>(Func<T, bool> func) where T : class, IEntity
    {
        var where = await GetWhere(func);

        return where.First();
    }


    public async Task<IEnumerable<T>> GetAll<T>() where T : class
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();

        return await connection.GetAllAsync<T>();
    }


    public async Task Save<T>(T entity) where T : class, IEntity
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();

        if (await Get<T>(entity.Id) is null)
        {
            await connection.InsertAsync(entity);

            return;
        }

        await connection.UpdateAsync(entity);
    }

    public async Task<T> Get<T>(int id) where T : class
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();
        return await connection.GetAsync<T>(id);
    }

    public async Task Delete<T>(T entity) where T : class, IEntity
    {
        await using var connection = new MySqlConnection(_dapperSettings.ConnectionString);

        await connection.OpenAsync();

        await connection.DeleteAsync(entity);
    }
}