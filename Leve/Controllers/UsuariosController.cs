using Leve.Data;
using Leve.Models;
using Leve.Services;
using Leve.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Leve.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly AppDbContext _context;
    private readonly SenhaService _senhaService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public UsuariosController(
        AppDbContext context,
        SenhaService senhaService,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _senhaService = senhaService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public IActionResult Cadastrar()
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(UsuarioFormularioViewModel model)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        ValidarDataNascimento(model.DataNascimento);
        await ValidarEmailDuplicadoAsync(model.Email);
        ValidarSenhaObrigatoriaCadastro(model);
        ValidarFotoObrigatoriaCadastro(model);

        if (model.FotoArquivo is not null && model.FotoArquivo.Length > 0 && !ArquivoEhImagemValida(model.FotoArquivo))
        {
            ModelState.AddModelError(nameof(model.FotoArquivo), "Envie uma imagem válida (JPG, JPEG, PNG ou WEBP).");
        }

        if (!ModelState.IsValid)
            return View(model);

        var nomeArquivoFoto = await SalvarFotoAsync(model.FotoArquivo!);

        var usuario = new Usuario
        {
            NomeCompleto = model.NomeCompleto,
            DataNascimento = model.DataNascimento,
            TelefoneFixo = model.TelefoneFixo,
            TelefoneCelular = model.TelefoneCelular,
            Email = model.Email,
            Endereco = model.Endereco,
            FotoNomeArquivo = nomeArquivoFoto,
            IsGestor = model.IsGestor
        };

        usuario.SenhaHash = _senhaService.GerarHash(usuario, model.Senha!);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Usuário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        const int itensPorPagina = 3;

        if (pagina < 1)
            pagina = 1;

        var totalItens = await _context.Usuarios.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);

        if (totalPaginas == 0)
            totalPaginas = 1;

        if (pagina > totalPaginas)
            pagina = totalPaginas;

        var usuarios = await _context.Usuarios
            .OrderBy(u => u.NomeCompleto)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        var viewModel = new UsuarioListaViewModel
        {
            Usuarios = usuarios,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var usuario = await ObterUsuarioAsync(id);

        if (usuario is null)
            return NotFound();

        return View(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var usuario = await ObterUsuarioAsync(id);

        if (usuario is null)
            return NotFound();

        var model = MapearParaFormulario(usuario);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(UsuarioFormularioViewModel model)
    {
        var acessoNegado = ValidarAcessoGestor();
        if (acessoNegado is not null)
            return acessoNegado;

        var usuario = await ObterUsuarioAsync(model.Id);

        if (usuario is null)
            return NotFound();

        ValidarDataNascimento(model.DataNascimento);
        await ValidarEmailDuplicadoAsync(model.Email, model.Id);

        if (model.FotoArquivo is not null && model.FotoArquivo.Length > 0 && !ArquivoEhImagemValida(model.FotoArquivo))
        {
            ModelState.AddModelError(nameof(model.FotoArquivo), "Envie uma imagem válida (JPG, JPEG, PNG ou WEBP).");
        }

        if (!string.IsNullOrWhiteSpace(model.Senha) && string.IsNullOrWhiteSpace(model.ConfirmarSenha))
        {
            ModelState.AddModelError(nameof(model.ConfirmarSenha), "Confirme a senha.");
        }

        if (!ModelState.IsValid)
            return View(model);

        usuario.NomeCompleto = model.NomeCompleto;
        usuario.DataNascimento = model.DataNascimento;
        usuario.TelefoneFixo = model.TelefoneFixo;
        usuario.TelefoneCelular = model.TelefoneCelular;
        usuario.Email = model.Email;
        usuario.Endereco = model.Endereco;
        usuario.IsGestor = model.IsGestor;

        if (model.FotoArquivo is not null && model.FotoArquivo.Length > 0)
        {
            var nomeArquivoFoto = await SalvarFotoAsync(model.FotoArquivo);
            usuario.FotoNomeArquivo = nomeArquivoFoto;
        }

        if (!string.IsNullOrWhiteSpace(model.Senha))
        {
            usuario.SenhaHash = _senhaService.GerarHash(usuario, model.Senha);
        }

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id = usuario.Id });
    }

    private IActionResult? ValidarAcessoGestor()
    {
        if (!UsuarioLogadoEhGestor())
            return RedirectToAction("AcessoNegado", "Autenticacao");

        return null;
    }

    private void ValidarDataNascimento(DateTime? dataNascimento)
    {
        if (dataNascimento.HasValue && dataNascimento.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(UsuarioFormularioViewModel.DataNascimento), "A data de nascimento não pode ser futura.");
        }
    }

    private async Task ValidarEmailDuplicadoAsync(string email, int? idIgnorado = null)
    {
        var emailEmUso = await _context.Usuarios.AnyAsync(u =>
            u.Email == email && (!idIgnorado.HasValue || u.Id != idIgnorado.Value));

        if (emailEmUso)
        {
            ModelState.AddModelError(nameof(UsuarioFormularioViewModel.Email), "Já existe um usuário cadastrado com este e-mail.");
        }
    }

    private void ValidarSenhaObrigatoriaCadastro(UsuarioFormularioViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            ModelState.AddModelError(nameof(model.Senha), "Informe a senha.");
        }

        if (string.IsNullOrWhiteSpace(model.ConfirmarSenha))
        {
            ModelState.AddModelError(nameof(model.ConfirmarSenha), "Confirme a senha.");
        }
    }

    private void ValidarFotoObrigatoriaCadastro(UsuarioFormularioViewModel model)
    {
        if (model.FotoArquivo is null || model.FotoArquivo.Length == 0)
        {
            ModelState.AddModelError(nameof(model.FotoArquivo), "Selecione uma foto.");
        }
    }

    private async Task<Usuario?> ObterUsuarioAsync(int id)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
    }

    private UsuarioFormularioViewModel MapearParaFormulario(Usuario usuario)
    {
        return new UsuarioFormularioViewModel
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            DataNascimento = usuario.DataNascimento,
            TelefoneFixo = usuario.TelefoneFixo,
            TelefoneCelular = usuario.TelefoneCelular,
            Email = usuario.Email,
            Endereco = usuario.Endereco,
            IsGestor = usuario.IsGestor,
            FotoNomeArquivoAtual = usuario.FotoNomeArquivo
        };
    }

    private bool UsuarioLogadoEhGestor()
    {
        return User.Identity?.IsAuthenticated == true
               && bool.TryParse(User.FindFirst("IsGestor")?.Value, out var isGestor)
               && isGestor;
    }

    private bool ArquivoEhImagemValida(IFormFile arquivo)
    {
        var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

        return extensoesPermitidas.Contains(extensao);
    }

    private async Task<string> SalvarFotoAsync(IFormFile fotoArquivo)
    {
        var pastaFotos = Path.Combine(_webHostEnvironment.WebRootPath, "imagens", "usuarios");

        if (!Directory.Exists(pastaFotos))
        {
            Directory.CreateDirectory(pastaFotos);
        }

        var extensao = Path.GetExtension(fotoArquivo.FileName).ToLowerInvariant();
        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(pastaFotos, nomeArquivo);

        using var stream = new FileStream(caminhoCompleto, FileMode.Create);
        await fotoArquivo.CopyToAsync(stream);

        return nomeArquivo;
    }
}