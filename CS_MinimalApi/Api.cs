using System.Runtime.CompilerServices;

namespace CS_MinimalApi;
/*
    All Handlers that handle any Routes like /api or /api/login etc come here.
    - if needed Middleware gets used here aswell
 */

public static class Api
{
    public static void SetupApiRoutes(this WebApplication app)
    {
        // mapping the Routes/enpoints to the Api methods
        app.MapGet("/Users", Handle_GetUsers);
        app.MapGet("/Users/{id}", Handle_GetUser);      // id from the Url (because no body for Getrequest)
        app.MapPost("/Users", Handle_InsertUser);       // user in body - body gets passed down and json parsed automagically? TODO: read about how that
        app.MapPut("/Users", Handle_UpdateUser);        // user in body
        app.MapDelete("/Users", Handle_DeletetUser);    // id in json from in body
    }

    private static async Task<IResult> Handle_GetUsers(IUserData data)
    {
        try
        {
            return Results.Ok(await data.GetUsers());
        }catch (Exception ex)
        {
            return Results.Problem(ex.Message); // just throw the error back to client
        }
    }

    private static async Task<IResult> Handle_GetUser(int id, IUserData data)
    // we get the id from the Url
    // and the data from the singleton we injected into app in Programm.Main
    {
        try
        {
            var res = await data.GetUser(id);
            if (res == null) return Results.NotFound();
            return Results.Ok(res);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> Handle_InsertUser(UserModel user, IUserData data)
    // we get the UserModel from the Body of the http-request (the data again from the singleton)
    {
        try
        {
            await data.InsertUser(user);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> Handle_UpdateUser(UserModel user, IUserData data)
    {
        try
        {
            await data.UpdateUser(user);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> Handle_DeletetUser(int userId, IUserData data)
    {
        try
        {
            await data.DeleteUser(userId);
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }


}
