using Leve.Models;
using Microsoft.AspNetCore.Identity;

namespace Leve.Services;

public class SenhaService
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string GerarHash(Usuario usuario, string senha)
    {
        return _passwordHasher.HashPassword(usuario, senha);
    }

    public bool VerificarSenha(Usuario usuario, string senhaInformada)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senhaInformada);

        return resultado == PasswordVerificationResult.Success
            || resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }
}