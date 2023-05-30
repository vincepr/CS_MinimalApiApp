# CS_MinimalAPI

## Infos
Goal is learning some Csharp basics, about APIs, SQL-Integration, Swagger etc. Coding along loosely (at least in the beginning) with the Youtube Project from [IAmTimCorey](https://www.youtube.com/watch?v=dwMFg6uxQ0I)

## Goal
Should just be a simple crud API

## Notes on Setting the Project
First we create the `ASP.NET Core Web App` then the `SQL Server Database Project` and a `class library` to define the shape of our data.

### SQL Server Database Project
- had to change to SQL Server 2019, to get it to work in Win11 (might have had to do something with disk sector size for win11 having changed the default settings, but whatever for now)
- add the Tables we want
- add `StoredProcedures` for all our incoming crud requests. (more optimized than just plain incoming sql queries)
example for the `spUser_Update.sql` (User is in squareBrackets because it is a reserved keyword)
```sql
CREATE PROCEDURE [dbo].[spUser_Update]
	@Id int,
	@FirstName nvarchar(50),
	@LastName nvarchar(50)
AS
BEGIN
	UPDATE dbo.[USER]
	SET Firstname = @FirstName, LastName = @LastName
	WHERE Id = @Id;
END
```
- next we a `Script` to the database-project. A post-deployment-script to run after the db is up.
```sql
/*  if were empty we fill some sample data for testing */
if not exists (select 1 from dbo.[User])
begin
    insert into dbo.[User] (FirstName, LastName)
    values('Tim', 'Hames'),('Bob', 'Ross'),('Amber','Spender'),('Cameron','Griffin');
end
```
- finally we rightclick the db-project and `Publish`
    -  Edit - Browse - Local - Select the local MSSQLLocalDB
    - give the local DB a name then `Save Profile As` into the db-project folder. Now we can re-publish after making changes etc.

The DB should be ready now. We can View - SQL Server Object Explorer. Open the localdb - Databases - nameOfDb - Tables - dbo.User rightclick and showData to check if it worked.

### Class Library
We create the class library and name it DataAccess. Will define the shape of the data in our App/Api
```cs
namespace DataAccess.Models;
internal class User{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```
#### Importing some packages/libraries
rightClick Dependencies in the DataAccess class library - Manage NuGet Packages. We add
- Dapper (Micro-ORM supporting different SQL Servers)
- System.Data.SqlClient
- Microsoft.Extensions.Configuration.Abstractions

### Data access library
SqöDataAccess.cs
```cs
using Dapper;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DataAccess.DbAccess;
public class SqlDataAccess
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
```
- we extract out our interfaces for easy conveniant use as dependency injection. In Visual Studio we can just select the public class `SqlDataAccess` and then press ctrl + dot then `extract interface` to autogenerate:

```cs
namespace DataAccess.DbAccess
public interface ISqlDataAccess
{
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default");
    Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default");
}
// here we could easy switch to another db just by implementing another interface like this (for mongodb/redis for ex.)
```

- now we implement all our usecases:
```cs
namespace DataAccess.Data;
public class UserData
{
    private readonly ISqlDataAccess _db;

    public UserData(ISqlDataAccess db)
    {
        _db = db;
    }

    public Task<IEnumerable<UserModel>> GetUsers() =>
        _db.LoadData<UserModel, dynamic>("dbo.spUser_GetAll", new { });

    public async Task<UserModel?> GetUser(int id)
    {
        var res = await _db.LoadData<UserModel, dynamic>("dbo.spUser_Get", new { Id = id });
        return res.FirstOrDefault();        // default for our UserModel is null
    }

    public Task InsertUser(UserModel user) => 
        _db.SaveData("dbo.spUser_Insert", new {user.FirstName, user.LastName});

    public Task UpdateUser(UserModel user) =>
        _db.SaveData("dbo.spUser_Update", user);

    public Task DeleteUser(int id) =>
        _db.SaveData("dbo.spUser_Delete", new {Id=id});
}
```