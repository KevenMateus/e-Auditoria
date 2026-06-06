using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EAuditoria.Infrastructure.Data.Migrations
{
    [DbContext(typeof(EAuditoria.Infrastructure.Data.AppDbContext))]
    [Migration("20250101000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    razao_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    regime_tributario = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obrigacoes_acessorias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    periodicidade = table.Column<int>(type: "integer", nullable: false),
                    competencia = table.Column<int>(type: "integer", nullable: false),
                    ano_competencia = table.Column<int>(type: "integer", nullable: false),
                    vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obrigacoes_acessorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_obrigacoes_acessorias_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entregas_obrigacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    obrigacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entregas_obrigacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_entregas_obrigacoes_obrigacoes_acessorias_obrigacao_id",
                        column: x => x.obrigacao_id,
                        principalTable: "obrigacoes_acessorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_empresas_cnpj",
                table: "empresas",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entregas_obrigacao_id",
                table: "entregas_obrigacoes",
                column: "obrigacao_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_obrigacoes_empresa_mes_ano",
                table: "obrigacoes_acessorias",
                columns: new[] { "empresa_id", "competencia", "ano_competencia" });

            migrationBuilder.CreateIndex(
                name: "ix_obrigacoes_empresa_tipo_competencia",
                table: "obrigacoes_acessorias",
                columns: new[] { "empresa_id", "tipo", "competencia", "ano_competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_obrigacoes_vencimento_status",
                table: "obrigacoes_acessorias",
                columns: new[] { "vencimento", "status" });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    perfil = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_login_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "usuarios");
            migrationBuilder.DropTable(name: "entregas_obrigacoes");
            migrationBuilder.DropTable(name: "obrigacoes_acessorias");
            migrationBuilder.DropTable(name: "empresas");
        }
    }
}
