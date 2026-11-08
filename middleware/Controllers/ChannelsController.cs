using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using middleware.Models;
using middleware.DTOs;
namespace middleware.Controllers;

[ApiController]
[Route("api/solid/{appName}")]
public class ChannelsController : ControllerBase
{
    private readonly SolidDatabase database;

    public ChannelsController(SolidDatabase database)
    {
        this.database = database;
    }

    [HttpPost]
    public IActionResult Create(string appName,[FromBody] ChannelRequest request){

        if (request.ResType != "channel")
        {
            return BadRequest("Invalid res-type");
        }

        if (string.IsNullOrWhiteSpace(request.ResourceName) ||
            string.IsNullOrWhiteSpace(request.ResourceDesc))
        {
            return BadRequest("Dados inválidos");
        }

        try
        {
            int? id = database.InsertChannel(
                appName,
                request.ResourceName,
                request.ResourceDesc
            );

            if (id == null)
                return NotFound("Aplicação não encontrada");

            var channel = database.GetChannel(
                appName,
                request.ResourceName
            );

            return Created(
                $"/api/solid/{Uri.EscapeDataString(appName)}/{Uri.EscapeDataString(request.ResourceName)}",
                channel
            );
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("Não foi possível criar o canal devido a uma restrição da base de dados");
        }
    }

    [HttpGet("{channelName}")]
    public IActionResult Get(
        string appName,
        string channelName,
        [FromHeader(Name = "solid-discovery")] string? discovery)
    {
        var channel = database.GetChannel(appName, channelName);

        if (channel == null)
            return NotFound();

        if (discovery == null)
            return Ok(channel);

        switch (discovery.ToLowerInvariant())
        {
            case "data-record":
                return Ok(database.DiscoverDataRecords(
                    appName, channelName));

            case "subscription":
                return Ok(database.DiscoverSubscriptions(
                    appName, channelName));

            case "application":
            case "channel":
                return Ok(Array.Empty<string>());

            default:
                return BadRequest("Invalid discovery type");
        }
    }

    
    [HttpPut("{channelName}")]
    public IActionResult Update(
        string appName,
        string channelName,
        [FromBody] ChannelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResourceName) ||
            string.IsNullOrWhiteSpace(request.ResourceDesc))
        {
            return BadRequest("Dados inválidos");
        }

        try
        {
            bool updated = database.UpdateChannel(
                appName,
                channelName,
                request.ResourceName,
                request.ResourceDesc
            );

            if (!updated)
                return NotFound();

            return Ok(database.GetChannel(appName, request.ResourceName));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("Atualização impedida por uma restrição da base de dados");
        }
    }

    [HttpDelete("{channelName}")]
    public IActionResult Delete(string appName, string channelName)
    {
        try
        {
            bool deleted = database.DeleteChannel(appName, channelName);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("O canal tem recursos associados que impedem a eliminação");
        }
    }

  

}