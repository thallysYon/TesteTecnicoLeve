using System.Security.Claims;
using Leve.Data;
using Leve.Enums;
using Leve.Models;
using Leve.Services;
using Leve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Leve.Controllers;

[Authorize]
public class TarefasController : Controller
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public TarefasController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        const int itensPorPagina = 6;

        if (pagina < 1)
            pagina = 1;

        var usuarioId = UsuarioLogadoId();
        var isGestor = UsuarioLogadoEhGestor();

        IQueryable<Tarefa> query = _context.Tarefas
            .Include(t => t.Responsavel)
            .Include(t => t.GestorCriador);

        query = isGestor
            ? query.Where(t => t.GestorCriadorId == usuarioId || t.ResponsavelId == usuarioId)
            : query.Where(t => t.ResponsavelId == usuarioId);

        var totalItens = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);

        if (totalPaginas == 0)
            totalPaginas = 1;

        if (pagina > totalPaginas)
            pagina = totalPaginas;

        var tarefas = await query
            .OrderBy(t => t.Status)
            .ThenBy(t => t.DataLimite)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var viewModel = new TarefaListaViewModel
        {
            Tarefas = tarefas,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Cadastrar()
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var model = new TarefaFormularioViewModel();
        await PreencherResponsaveisAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(TarefaFormularioViewModel model)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        ValidarDataLimite(model.DataLimite);

        var responsavel = await ObterResponsavelValidoAsync(model.ResponsavelId);
        if (responsavel is null)
        {
            ModelState.AddModelError(nameof(model.ResponsavelId), "Selecione um responsável válido.");
        }

        if (!ModelState.IsValid)
        {
            await PreencherResponsaveisAsync(model);
            return View(model);
        }

        var tarefa = new Tarefa
        {
            Mensagem = model.Mensagem,
            DataLimite = model.DataLimite,
            Status = StatusTarefa.Pendente,
            DataCriacao = DateTime.Now,
            ResponsavelId = model.ResponsavelId,
            GestorCriadorId = UsuarioLogadoId()
        };

        _context.Tarefas.Add(tarefa);
        await _context.SaveChangesAsync();

        var assunto = "Nova tarefa atribuída";
        var corpo = $@"
            <h2>Você recebeu uma nova tarefa</h2>
            <p><strong>Descrição:</strong> {tarefa.Mensagem}</p>
            <p><strong>Data limite:</strong> {tarefa.DataLimite?.ToString("dd/MM/yyyy")}</p>";

        await TentarEnviarEmailAsync(responsavel?.Email, assunto, corpo);

        TempData["Sucesso"] = "Tarefa cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int id)
    {
        var usuarioId = UsuarioLogadoId();

        var tarefa = await _context.Tarefas
            .Include(t => t.GestorCriador)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tarefa is null)
            return NotFound();

        if (tarefa.ResponsavelId != usuarioId)
            return RedirectToAction("AcessoNegado", "Autenticacao");

        if (tarefa.Status == StatusTarefa.Concluida)
            return RedirectToAction(nameof(Index));

        tarefa.Status = StatusTarefa.Concluida;
        tarefa.DataConclusao = DateTime.Now;

        await _context.SaveChangesAsync();

        var assunto = "Tarefa concluída";
        var corpo = $@"
            <h2>Uma tarefa foi concluída</h2>
            <p><strong>Descrição:</strong> {tarefa.Mensagem}</p>
            <p><strong>Responsável:</strong> {User.Identity?.Name}</p>
            <p><strong>Concluída em:</strong> {tarefa.DataConclusao?.ToString("dd/MM/yyyy HH:mm")}</p>";

        await TentarEnviarEmailAsync(tarefa.GestorCriador?.Email, assunto, corpo);

        TempData["Sucesso"] = "Tarefa concluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        var usuarioId = UsuarioLogadoId();
        var isGestor = UsuarioLogadoEhGestor();

        var tarefa = await ObterTarefaCompletaAsync(id);

        if (tarefa is null)
            return NotFound();

        var podeVisualizar =
            tarefa.ResponsavelId == usuarioId ||
            tarefa.GestorCriadorId == usuarioId ||
            isGestor;

        if (!podeVisualizar)
            return RedirectToAction("AcessoNegado", "Autenticacao");

        return View(tarefa);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var tarefa = await ObterTarefaAsync(id);

        if (tarefa is null)
            return NotFound();

        var model = new TarefaFormularioViewModel
        {
            ResponsavelId = tarefa.ResponsavelId,
            Mensagem = tarefa.Mensagem,
            DataLimite = tarefa.DataLimite
        };

        await PreencherResponsaveisAsync(model);

        ViewBag.TarefaId = tarefa.Id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, TarefaFormularioViewModel model)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var tarefa = await ObterTarefaAsync(id);

        if (tarefa is null)
            return NotFound();

        ValidarDataLimite(model.DataLimite);

        var responsavel = await ObterResponsavelValidoAsync(model.ResponsavelId);
        if (responsavel is null)
        {
            ModelState.AddModelError(nameof(model.ResponsavelId), "Selecione um responsável válido.");
        }

        if (!ModelState.IsValid)
        {
            await PreencherResponsaveisAsync(model);
            ViewBag.TarefaId = id;
            return View(model);
        }

        tarefa.ResponsavelId = model.ResponsavelId;
        tarefa.Mensagem = model.Mensagem;
        tarefa.DataLimite = model.DataLimite;

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Tarefa atualizada com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id = tarefa.Id });
    }

    private IActionResult? ValidarAcessoGestor()
    {
        if (!UsuarioLogadoEhGestor())
            return RedirectToAction("AcessoNegado", "Autenticacao");

        return null;
    }

    private void ValidarDataLimite(DateTime? dataLimite)
    {
        if (dataLimite.HasValue && dataLimite.Value.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(TarefaFormularioViewModel.DataLimite), "A data limite não pode ser anterior à data atual.");
        }
    }

    private async Task PreencherResponsaveisAsync(TarefaFormularioViewModel model)
    {
        model.Responsaveis = await ObterResponsaveisAsync();
    }

    private async Task<Usuario?> ObterResponsavelValidoAsync(int responsavelId)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == responsavelId);
    }

    private async Task<Tarefa?> ObterTarefaAsync(int id)
    {
        return await _context.Tarefas.FirstOrDefaultAsync(t => t.Id == id);
    }

    private async Task<Tarefa?> ObterTarefaCompletaAsync(int id)
    {
        return await _context.Tarefas
            .Include(t => t.Responsavel)
            .Include(t => t.GestorCriador)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    private async Task TentarEnviarEmailAsync(string? destinatario, string assunto, string corpoHtml)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
            return;

        try
        {
            await _emailService.EnviarAsync(destinatario, assunto, corpoHtml);
        }
        catch
        {
            // Não quebra o fluxo principal se o e-mail falhar.
        }
    }

    private async Task<List<SelectListItem>> ObterResponsaveisAsync()
    {
        return await _context.Usuarios
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.NomeCompleto
            })
            .ToListAsync();
    }

    private int UsuarioLogadoId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private bool UsuarioLogadoEhGestor()
    {
        return bool.TryParse(User.FindFirst("IsGestor")?.Value, out var isGestor) && isGestor;
    }
}