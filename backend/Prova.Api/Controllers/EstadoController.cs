using Microsoft.AspNetCore.Mvc;
using Prova.Service.Dtos;
using Prova.Service.Services;

namespace Prova.Api.Controllers;

/// <summary>
/// Endpoint somente leitura de Estado (com Cidades aninhadas) para alimentar
/// o combobox em cascata do frontend (ver <see cref="IEstadoService"/>). Não
/// há CidadeController separado — um único GET já entrega os dois níveis.
/// </summary>
[ApiController]
[Route("api/estados")]
public class EstadoController : ControllerBase
{
    private readonly IEstadoService _estadoService;

    public EstadoController(IEstadoService estadoService)
    {
        _estadoService = estadoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EstadoComCidadesDto>>> ObterTodos()
    {
        var estados = await _estadoService.ObterEstadosComCidadesAsync();
        return Ok(estados);
    }
}
