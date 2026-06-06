using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Gestão de empresas cadastradas no escritório contábil.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Empresas")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresasController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    /// <summary>
    /// Lista todas as empresas ativas.
    /// </summary>
    /// <remarks>
    /// Retorna apenas empresas com <c>ativo = true</c>, ordenadas por razão social.
    ///
    ///     GET /api/empresas
    ///
    /// </remarks>
    /// <returns>Lista de empresas ativas.</returns>
    /// <response code="200">Lista retornada com sucesso (pode ser vazia).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmpresaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmpresaResponse>>> Listar()
    {
        var empresas = await _empresaService.ListarAsync();
        return Ok(empresas);
    }

    /// <summary>
    /// Obtém uma empresa pelo Id.
    /// </summary>
    /// <param name="id">UUID da empresa.</param>
    /// <returns>Dados da empresa.</returns>
    /// <response code="200">Empresa encontrada.</response>
    /// <response code="404">Empresa não encontrada ou inativa.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmpresaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpresaResponse>> ObterPorId(Guid id)
    {
        var empresa = await _empresaService.ObterPorIdAsync(id);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    /// <summary>
    /// Cadastra uma nova empresa.
    /// </summary>
    /// <remarks>
    /// O CNPJ pode ser enviado com ou sem formatação (pontos, barra e hífen são removidos automaticamente).
    ///
    ///     POST /api/empresas
    ///     {
    ///         "razaoSocial": "Tech Solutions ME",
    ///         "cnpj": "22.333.444/0001-95",
    ///         "regimeTributario": "SimplesNacional"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Dados da nova empresa.</param>
    /// <returns>Empresa criada com Id gerado.</returns>
    /// <response code="201">Empresa criada com sucesso.</response>
    /// <response code="422">CNPJ já cadastrado ou dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmpresaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmpresaResponse>> Criar([FromBody] CriarEmpresaRequest request)
    {
        var empresa = await _empresaService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = empresa.Id }, empresa);
    }

    /// <summary>
    /// Atualiza razão social e/ou regime tributário.
    /// </summary>
    /// <remarks>
    /// Não é possível alterar o CNPJ após o cadastro.
    ///
    ///     PUT /api/empresas/{id}
    ///     {
    ///         "razaoSocial": "Tech Solutions Ltda",
    ///         "regimeTributario": "LucroPresumido"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">UUID da empresa a atualizar.</param>
    /// <param name="request">Campos a atualizar.</param>
    /// <returns>Empresa atualizada.</returns>
    /// <response code="200">Empresa atualizada com sucesso.</response>
    /// <response code="404">Empresa não encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmpresaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpresaResponse>> Atualizar(Guid id, [FromBody] AtualizarEmpresaRequest request)
    {
        var empresa = await _empresaService.AtualizarAsync(id, request);
        return Ok(empresa);
    }

    /// <summary>
    /// Remove (desativa) uma empresa.
    /// </summary>
    /// <remarks>
    /// Operação de soft-delete: a empresa não é apagada do banco, apenas marcada como <c>ativo = false</c>.
    /// Todas as obrigações vinculadas são preservadas.
    /// </remarks>
    /// <param name="id">UUID da empresa a remover.</param>
    /// <response code="204">Empresa desativada com sucesso.</response>
    /// <response code="404">Empresa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id)
    {
        await _empresaService.RemoverAsync(id);
        return NoContent();
    }
}
