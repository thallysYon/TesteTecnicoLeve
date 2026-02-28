using Leve.Models;

namespace Leve.ViewModels;

public class UsuarioListaViewModel
{
    public List<Usuario> Usuarios { get; set; } = new();
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
}