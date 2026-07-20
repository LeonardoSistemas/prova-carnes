using System.Net;
using System.Net.Http.Json;
using Prova.Api.Tests.Infrastructure;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Controllers;

/// <summary>
/// Teste de integração de <c>EstadoController</c> (T21/T18): endpoint
/// somente leitura, sem regra de negócio própria — cobre apenas o fluxo
/// feliz (200 com Estado trazendo suas Cidades aninhadas).
/// </summary>
public class EstadoControllerTests : IDisposable
{
    private readonly ProvaApiFactory _factory = new();
    private readonly HttpClient _client;

    public EstadoControllerTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_ComCidadeSemeada_Retorna200ComEstadoECidadeAninhada()
    {
        var cidade = await _factory.SemearCidadeAsync();

        var resposta = await _client.GetAsync("/api/estados");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var estados = await resposta.Content.ReadFromJsonAsync<List<EstadoComCidadesDto>>();
        Assert.NotNull(estados);
        Assert.Single(estados!);
        Assert.Single(estados![0].Cidades);
        Assert.Equal(cidade.Nome, estados[0].Cidades[0].Nome);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
