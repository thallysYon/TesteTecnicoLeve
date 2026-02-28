using System.ComponentModel.DataAnnotations;

namespace Leve.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome completo deve ter entre 3 e 150 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date, ErrorMessage = "Informe uma data válida.")]
    public DateTime? DataNascimento { get; set; }

    [Phone(ErrorMessage = "Informe um telefone fixo válido.")]
    [StringLength(10, ErrorMessage = "O telefone fixo deve ter no máximo 10 caracteres.")]
    public string? TelefoneFixo { get; set; }

    [Phone(ErrorMessage = "Informe um telefone celular válido.")]
    [StringLength(11, ErrorMessage = "O telefone celular deve ter no máximo 11 caracteres.")]
    public string? TelefoneCelular { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereço.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "O endereço deve ter entre 5 e 250 caracteres.")]
    public string Endereco { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "O nome do arquivo da foto deve ter no máximo 255 caracteres.")]
    public string? FotoNomeArquivo { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "O hash da senha deve ter no máximo 500 caracteres.")]
    public string SenhaHash { get; set; } = string.Empty;

    public bool IsGestor { get; set; }
}