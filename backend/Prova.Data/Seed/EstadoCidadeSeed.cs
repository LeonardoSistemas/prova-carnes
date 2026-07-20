using Prova.Model.Entities;

namespace Prova.Data.Seed;

/// <summary>
/// Dados semeados via <c>HasData</c> para as entidades de apoio Estado e
/// Cidade (T07). Ids fixados explicitamente porque <c>HasData</c> exige
/// chave estável entre migrations — nunca devem ser renumerados depois de
/// uma migration já aplicada em algum ambiente.
///
/// Os objetos aqui NÃO podem preencher propriedades de navegação
/// (<see cref="Estado.Cidades"/>, <see cref="Cidade.Estado"/>) — o EF Core
/// não aceita grafo de navegação em <c>HasData</c>, só as colunas
/// escalares/FK.
/// </summary>
public static class EstadoCidadeSeed
{
    public static readonly Estado[] Estados =
    {
        new() { Id = 1, Nome = "São Paulo", Uf = "SP" },
        new() { Id = 2, Nome = "Rio de Janeiro", Uf = "RJ" },
        new() { Id = 3, Nome = "Minas Gerais", Uf = "MG" },
        new() { Id = 4, Nome = "Rio Grande do Sul", Uf = "RS" },
        new() { Id = 5, Nome = "Paraná", Uf = "PR" },
        new() { Id = 6, Nome = "Bahia", Uf = "BA" },
        new() { Id = 7, Nome = "Distrito Federal", Uf = "DF" },
        new() { Id = 8, Nome = "Santa Catarina", Uf = "SC" },
        new() { Id = 9, Nome = "Pernambuco", Uf = "PE" },
        new() { Id = 10, Nome = "Ceará", Uf = "CE" },
        new() { Id = 11, Nome = "Goiás", Uf = "GO" },
        new() { Id = 12, Nome = "Espírito Santo", Uf = "ES" },
        new() { Id = 13, Nome = "Pará", Uf = "PA" },
        new() { Id = 14, Nome = "Amazonas", Uf = "AM" },
        new() { Id = 15, Nome = "Mato Grosso", Uf = "MT" },
        new() { Id = 16, Nome = "Mato Grosso do Sul", Uf = "MS" },
        new() { Id = 17, Nome = "Maranhão", Uf = "MA" },
        new() { Id = 18, Nome = "Paraíba", Uf = "PB" },
        new() { Id = 19, Nome = "Rio Grande do Norte", Uf = "RN" },
        new() { Id = 20, Nome = "Alagoas", Uf = "AL" },
        new() { Id = 21, Nome = "Sergipe", Uf = "SE" },
        new() { Id = 22, Nome = "Piauí", Uf = "PI" },
        new() { Id = 23, Nome = "Tocantins", Uf = "TO" },
        new() { Id = 24, Nome = "Rondônia", Uf = "RO" },
        new() { Id = 25, Nome = "Roraima", Uf = "RR" },
        new() { Id = 26, Nome = "Acre", Uf = "AC" },
        new() { Id = 27, Nome = "Amapá", Uf = "AP" },
    };

    public static readonly Cidade[] Cidades =
    {
        // São Paulo (Id Estado = 1) — capital + 2 cidades adicionais
        new() { Id = 1, Nome = "São Paulo", EstadoId = 1 },
        new() { Id = 2, Nome = "Campinas", EstadoId = 1 },
        new() { Id = 3, Nome = "Guarulhos", EstadoId = 1 },

        // Rio de Janeiro (Id Estado = 2) — capital + 1 cidade adicional
        new() { Id = 4, Nome = "Rio de Janeiro", EstadoId = 2 },
        new() { Id = 5, Nome = "Niterói", EstadoId = 2 },

        // Minas Gerais (Id Estado = 3) — capital + 1 cidade adicional
        new() { Id = 6, Nome = "Belo Horizonte", EstadoId = 3 },
        new() { Id = 7, Nome = "Uberlândia", EstadoId = 3 },

        // Demais estados — capital
        new() { Id = 8, Nome = "Porto Alegre", EstadoId = 4 },
        new() { Id = 9, Nome = "Curitiba", EstadoId = 5 },
        new() { Id = 10, Nome = "Salvador", EstadoId = 6 },
        new() { Id = 11, Nome = "Brasília", EstadoId = 7 },
        new() { Id = 12, Nome = "Florianópolis", EstadoId = 8 },
        new() { Id = 13, Nome = "Recife", EstadoId = 9 },
        new() { Id = 14, Nome = "Fortaleza", EstadoId = 10 },
        new() { Id = 15, Nome = "Goiânia", EstadoId = 11 },
        new() { Id = 16, Nome = "Vitória", EstadoId = 12 },
        new() { Id = 17, Nome = "Belém", EstadoId = 13 },
        new() { Id = 18, Nome = "Manaus", EstadoId = 14 },
        new() { Id = 19, Nome = "Cuiabá", EstadoId = 15 },
        new() { Id = 20, Nome = "Campo Grande", EstadoId = 16 },
        new() { Id = 21, Nome = "São Luís", EstadoId = 17 },
        new() { Id = 22, Nome = "João Pessoa", EstadoId = 18 },
        new() { Id = 23, Nome = "Natal", EstadoId = 19 },
        new() { Id = 24, Nome = "Maceió", EstadoId = 20 },
        new() { Id = 25, Nome = "Aracaju", EstadoId = 21 },
        new() { Id = 26, Nome = "Teresina", EstadoId = 22 },
        new() { Id = 27, Nome = "Palmas", EstadoId = 23 },
        new() { Id = 28, Nome = "Porto Velho", EstadoId = 24 },
        new() { Id = 29, Nome = "Boa Vista", EstadoId = 25 },
        new() { Id = 30, Nome = "Rio Branco", EstadoId = 26 },
        new() { Id = 31, Nome = "Macapá", EstadoId = 27 },
    };
}
