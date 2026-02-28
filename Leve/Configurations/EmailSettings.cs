namespace Leve.Configurations;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string NomeRemetente { get; set; } = string.Empty;
    public string EmailRemetente { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool UsarSsl { get; set; }
}