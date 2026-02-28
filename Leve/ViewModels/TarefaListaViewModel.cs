using Leve.Models;

namespace Leve.ViewModels;

public class TarefaListaViewModel
{
    public List<Tarefa> Tarefas { get; set; } = new();
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
}