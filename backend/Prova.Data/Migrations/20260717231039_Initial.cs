using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Prova.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carnes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Origem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carnes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cidades_Estados_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "Estados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compradores_Cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "Cidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompradorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Compradores_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Compradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    CarneId = table.Column<int>(type: "int", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Moeda = table.Column<int>(type: "int", nullable: false),
                    CotacaoUsada = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoItens_Carnes_CarneId",
                        column: x => x.CarneId,
                        principalTable: "Carnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoItens_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Estados",
                columns: new[] { "Id", "Nome", "Uf" },
                values: new object[,]
                {
                    { 1, "São Paulo", "SP" },
                    { 2, "Rio de Janeiro", "RJ" },
                    { 3, "Minas Gerais", "MG" },
                    { 4, "Rio Grande do Sul", "RS" },
                    { 5, "Paraná", "PR" },
                    { 6, "Bahia", "BA" },
                    { 7, "Distrito Federal", "DF" },
                    { 8, "Santa Catarina", "SC" },
                    { 9, "Pernambuco", "PE" },
                    { 10, "Ceará", "CE" },
                    { 11, "Goiás", "GO" },
                    { 12, "Espírito Santo", "ES" },
                    { 13, "Pará", "PA" },
                    { 14, "Amazonas", "AM" },
                    { 15, "Mato Grosso", "MT" },
                    { 16, "Mato Grosso do Sul", "MS" },
                    { 17, "Maranhão", "MA" },
                    { 18, "Paraíba", "PB" },
                    { 19, "Rio Grande do Norte", "RN" },
                    { 20, "Alagoas", "AL" },
                    { 21, "Sergipe", "SE" },
                    { 22, "Piauí", "PI" },
                    { 23, "Tocantins", "TO" },
                    { 24, "Rondônia", "RO" },
                    { 25, "Roraima", "RR" },
                    { 26, "Acre", "AC" },
                    { 27, "Amapá", "AP" }
                });

            migrationBuilder.InsertData(
                table: "Cidades",
                columns: new[] { "Id", "EstadoId", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "São Paulo" },
                    { 2, 1, "Campinas" },
                    { 3, 1, "Guarulhos" },
                    { 4, 2, "Rio de Janeiro" },
                    { 5, 2, "Niterói" },
                    { 6, 3, "Belo Horizonte" },
                    { 7, 3, "Uberlândia" },
                    { 8, 4, "Porto Alegre" },
                    { 9, 5, "Curitiba" },
                    { 10, 6, "Salvador" },
                    { 11, 7, "Brasília" },
                    { 12, 8, "Florianópolis" },
                    { 13, 9, "Recife" },
                    { 14, 10, "Fortaleza" },
                    { 15, 11, "Goiânia" },
                    { 16, 12, "Vitória" },
                    { 17, 13, "Belém" },
                    { 18, 14, "Manaus" },
                    { 19, 15, "Cuiabá" },
                    { 20, 16, "Campo Grande" },
                    { 21, 17, "São Luís" },
                    { 22, 18, "João Pessoa" },
                    { 23, 19, "Natal" },
                    { 24, 20, "Maceió" },
                    { 25, 21, "Aracaju" },
                    { 26, 22, "Teresina" },
                    { 27, 23, "Palmas" },
                    { 28, 24, "Porto Velho" },
                    { 29, 25, "Boa Vista" },
                    { 30, 26, "Rio Branco" },
                    { 31, 27, "Macapá" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cidades_EstadoId",
                table: "Cidades",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_CidadeId",
                table: "Compradores",
                column: "CidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_CarneId",
                table: "PedidoItens",
                column: "CarneId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_PedidoId",
                table: "PedidoItens",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CompradorId",
                table: "Pedidos",
                column: "CompradorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoItens");

            migrationBuilder.DropTable(
                name: "Carnes");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Compradores");

            migrationBuilder.DropTable(
                name: "Cidades");

            migrationBuilder.DropTable(
                name: "Estados");
        }
    }
}
