namespace OrionPOS.Domain.Catalogo;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
