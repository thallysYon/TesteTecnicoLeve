using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Leve.ViewModels;

public class TarefaFormularioViewModel
{
    [Required(ErrorMessage = "Selecione um responsável.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um responsável válido.")]
    public int ResponsavelId { get; set; }

    [Required(ErrorMessage = "Informe a descrição da tarefa.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "A descrição da tarefa deve ter entre 3 e 500 caracteres.")]
    public string Mensagem { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data limite.")]
    [DataType(DataType.Date)]
    public DateTime? DataLimite { get; set; }

    public List<SelectListItem> Responsaveis { get; set; } = new();
}