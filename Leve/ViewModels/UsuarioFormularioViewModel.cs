using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Leve.ViewModels;

public class UsuarioFormularioViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome completo deve ter entre 3 e 150 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [Phone(ErrorMessage = "Informe um telefone fixo válido.")]
    [StringLength(20, ErrorMessage = "O telefone fixo deve ter no máximo 20 caracteres.")]
    public string? TelefoneFixo { get; set; }

    [Phone(ErrorMessage = "Informe um telefone celular válido.")]
    [StringLength(20, ErrorMessage = "O telefone celular deve ter no máximo 20 caracteres.")]
    public string? TelefoneCelular { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereço.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "O endereço deve ter entre 5 e 250 caracteres.")]
    public string Endereco { get; set; } = string.Empty;

    public IFormFile? FotoArquivo { get; set; }

    public string? FotoNomeArquivoAtual { get; set; }

    public bool IsGestor { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    public string? Senha { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não conferem.")]
    public string? ConfirmarSenha { get; set; }
}