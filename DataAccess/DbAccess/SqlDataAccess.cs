using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DataAccess.DbAccess;

public class SqlDataAccess : ISqlDataAccess
{
    // _ to indicate private vars coming over from Dependency Injection
    private readonly IConfiguration _config;

    public SqlDataAccess(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(
        string storedProcedure,             // storedProcedure name in our db
        U parameters,                       // (optional) params like the {id} for /Get?id=123
        string connectionId = "Default")    // info about what db-name/port/etc we are connecting to
                                            //  this is stored in the appsettings.json of our Api
    {
        // with the using here we garante (graceful) conn.Close() when leaving the scope
        // so we garantee 100%? were not leaving connections to the db open.
        using IDbConnection conn = new SqlConnection(_config.GetConnectionString(connectionId));
        return await conn.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task SaveData<T>(
        string storedProcedure,
        T parameters,
        string connectionId = "Default")
    {
        using IDbConnection conn = new SqlConnection(_config.GetConnectionString(connectionId));
        await conn.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
    }
}
