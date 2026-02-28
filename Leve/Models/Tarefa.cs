using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Leve.Enums;

namespace Leve.Models;

public class Tarefa
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a descrição da tarefa.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "A descrição da tarefa deve ter entre 3 e 500 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite.")]
    [DataType(DataType.Date, ErrorMessage = "Informe uma data válida.")]
    public DateTime? DataLimite { get; set; }

    [Required]
    public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;

    [Required]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public DateTime? DataConclusao { get; set; }

    [Required(ErrorMessage = "Selecione um responsável.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um responsável válido.")]
    public int ResponsavelId { get; set; }

    [ForeignKey(nameof(ResponsavelId))]
    public Usuario? Responsavel { get; set; }

    [Required(ErrorMessage = "Informe o gestor criador.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um gestor criador válido.")]
    public int GestorCriadorId { get; set; }

    [ForeignKey(nameof(GestorCriadorId))]
    public Usuario? GestorCriador { get; set; }
}