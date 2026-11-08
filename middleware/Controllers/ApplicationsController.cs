using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using middleware.Models;
using middleware.DTOs;

namespace middleware.Controllers;

[ApiController]
[Route("api/solid")]
public class ApplicationsController : ControllerBase
{
    private readonly SolidDatabase database;

    public ApplicationsController(SolidDatabase database)
    {
        this.database = database;
    }

    [HttpPost]
    public IActionResult Create([FromBody] ApplicationRequest request)
    {
        if (request.ResType != "application"){
            return BadRequest("Invalid res-type");
        }

        if (string.IsNullOrWhiteSpace(request.ResourceDesc))
        {
            return BadRequest("Dados inválidos");
        }

      

            //string resName = request.ResourceName ?? $"{request.ResourceDesc}{Random.Shared.Next(1,999)}" ;
            string resName = string.IsNullOrWhiteSpace(request.ResourceName)//evitar '' ou '    '
            ? $"app-{Guid.NewGuid():N}"
            : request.ResourceName.Trim();
         
          for (int attempt = 0; attempt < 5; attempt++) //resolver nomes repetidos
        
            try
            {
                database.InsertApplication(resName, request.ResourceDesc);

                var application = database.GetApplication(resName);

                // TODO: devolver 201 com o DTO público
                return Created(
                    $"/api/solid/{Uri.EscapeDataString(resName)}",
                    application
                );
            }
            catch (SqliteException ex) when (
                ex.SqliteErrorCode == 19 &&
                ex.SqliteExtendedErrorCode == 2067)
            {
                // Nome duplicado: gerar outro e tentar novamente
                resName = $"app-{Guid.NewGuid():N}";
            }
        

        return StatusCode(500, "Unable to generate a unique resource name");
    }

    [HttpGet("{appName}")]
    public IActionResult Get(
        string appName,
        [FromHeader(Name = "solid-discovery")] string? discovery)
    {
        var application = database.GetApplication(appName);

        if (application == null)
            return NotFound();

        if (discovery == "channel")
            return Ok(database.DiscoverChannels(appName));

        if (discovery != null)
            return BadRequest("Discovery inválido");

        return Ok(application);
    }

    [HttpPut("{appName}")]
    public IActionResult Update(
        string appName,
        [FromBody] ApplicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResourceName) ||
            string.IsNullOrWhiteSpace(request.ResourceDesc))
        {
            return BadRequest("Dados inválidos");
        }

        try
        {
            bool updated = database.UpdateApplication(
                appName,
                request.ResourceName,
                request.ResourceDesc
            );

            if (!updated)
                return NotFound();

            return Ok(database.GetApplication(request.ResourceName));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("Atualização impedida por uma restrição da base de dados");
        }
    }

    [HttpDelete("{appName}")]
    public IActionResult Delete(string appName)
    {
        try
        {
            bool deleted = database.DeleteApplication(appName);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return Conflict("A aplicação tem recursos associados que impedem a eliminação");
        }
    }

    [HttpGet]
    public IActionResult Discover(
        [FromHeader(Name = "solid-discovery")] string? resourceType)
    {
        if (resourceType == "application")
            return Ok(database.DiscoverApplications());

        if (resourceType == "channel")
            return Ok(database.DiscoverChannels());

        return BadRequest("Header solid-discovery inválido ou em falta");
    }
}