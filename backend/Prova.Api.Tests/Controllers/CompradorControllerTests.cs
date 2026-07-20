using System.Net;
using System.Net.Http.Json;
using Prova.Api.Tests.Infrastructure;
using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Controllers;

/// <summary>
/// Testes de integração de <c>CompradorController</c> (T21/T17): mesmos
/// critérios de status code de <c>CarneControllerTests</c>, mais o cenário
/// específico de referência inexistente (CidadeId) → 404 e delete bloqueado
/// por Pedido vinculado → 409.
/// </summary>
public class CompradorControllerTests : IDisposable
{
    private readonly ProvaApiFactory _factory = new();
    private readonly HttpClient _client;

    public CompradorControllerTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_SemCompradoresCadastrados_Retorna200ComArrayVazio()
    {
        var resposta = await _client.GetAsync("/api/compradores");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var compradores = await resposta.Content.ReadFromJsonAsync<List<CompradorResponseDto>>();
        Assert.NotNull(compradores);
        Assert.Empty(compradores!);
    }

    [Fact]
    public async Task Post_CompradorValido_Retorna201ComLocationHeader()
    {
        var cidade = await _factory.SemearCidadeAsync();
        var dto = new CompradorDto("João Comprador", "12345678900", cidade.Id);

        var resposta = await _client.PostAsJsonAsync("/api/compradores", dto);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);
    }

    [Fact]
    public async Task Post_CidadeInexistente_Retorna404()
    {
        var dto = new CompradorDto("Maria Compradora", "98765432100", CidadeId: 999999);

        var resposta = await _client.PostAsJsonAsync("/api/compradores", dto);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Put_IdInexistente_Retorna404()
    {
        var cidade = await _factory.SemearCidadeAsync();
        var dto = new CompradorDto("Nome Qualquer", "00000000000", cidade.Id);

        var resposta = await _client.PutAsJsonAsync("/api/compradores/999999", dto);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Delete_CompradorComPedidoVinculado_Retorna409()
    {
        var cidade = await _factory.SemearCidadeAsync();

        var carneCriada = await CriarCarneAsync();
        var compradorCriado = await CriarCompradorAsync(cidade.Id);

        var pedidoDto = new PedidoDto(
            DateTime.UtcNow,
            compradorCriado.Id,
            new List<PedidoItemDto> { new(carneCriada.Id, 50m, Moeda.BRL) });

        var respostaPedido = await _client.PostAsJsonAsync("/api/pedidos", pedidoDto);
        Assert.Equal(HttpStatusCode.Created, respostaPedido.StatusCode);

        var resposta = await _client.DeleteAsync($"/api/compradores/{compradorCriado.Id}");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    private async Task<CarneResponseDto> CriarCarneAsync()
    {
        var resposta = await _client.PostAsJsonAsync("/api/carnes", new CarneDto("Cupim", OrigemCarne.Bovina));
        return (await resposta.Content.ReadFromJsonAsync<CarneResponseDto>())!;
    }

    private async Task<CompradorResponseDto> CriarCompradorAsync(int cidadeId)
    {
        var resposta = await _client.PostAsJsonAsync(
            "/api/compradores", new CompradorDto("Comprador Vinculado", "11122233344", cidadeId));
        return (await resposta.Content.ReadFromJsonAsync<CompradorResponseDto>())!;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
