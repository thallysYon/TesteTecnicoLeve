using Leve.Models;
using Leve.Services;

namespace Leve.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context, SenhaService senhaService)
    {
        if (context.Usuarios.Any(u => u.Email == "ti@leveinvestimentos.com.br"))
            return;

        var gestor = new Usuario
        {
            NomeCompleto = "Gestor Inicial",
            DataNascimento = new DateTime(1990, 1, 1),
            Email = "ti@leveinvestimentos.com.br",
            Endereco = "Não informado",
            IsGestor = true
        };

        gestor.SenhaHash = senhaService.GerarHash(gestor, "teste123");

        context.Usuarios.Add(gestor);
        context.SaveChanges();
    }
}