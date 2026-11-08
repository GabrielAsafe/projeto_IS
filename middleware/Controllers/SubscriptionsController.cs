using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using middleware.Models;

namespace middleware.Controllers;




[ApiController]
[Route("api/solid")]
public class SubscriptionsController : ControllerBase
{

    private readonly SolidDatabase database;

    public SubscriptionsController(SolidDatabase database)
    {
        this.database = database;
    }



    // GET /api/solid/app1/channel1/subs/sub1
    [HttpGet("{appName}/{channelName}/subs/{subName}")]
    public IActionResult Get(
        string appName,
        string channelName,
        string subName)
    {
        // TODO: Consultar subscription

        var subs = database.GetSubscription(appName,channelName,subName);

        if (subs == null)
            return NotFound();

        return Ok(subs);

    }

    // DELETE /api/solid/app1/channel1/subs/sub1
    [HttpDelete("{appName}/{channelName}/subs/{subName}")]
    public IActionResult Delete(
        string appName,
        string channelName,
        string subName)
    {
         try
        {
            bool deleted = database.DeleteSubscription(appName,channelName,subName);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("A aplicação tem recursos associados que impedem a eliminação");
        }
    }
}