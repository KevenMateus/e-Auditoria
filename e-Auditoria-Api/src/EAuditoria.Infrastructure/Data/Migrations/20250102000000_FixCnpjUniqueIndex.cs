// Arquivo intencionalmente vazio — migration cancelada.
// A regra de unicidade de CNPJ é tratada em nível de aplicação,
// detectando empresas inativas e oferecendo reativação ao usuário.
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EAuditoria.Infrastructure.Data.Migrations;

public partial class FixCnpjUniqueIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) { }
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
