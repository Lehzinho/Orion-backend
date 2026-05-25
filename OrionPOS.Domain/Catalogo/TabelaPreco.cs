namespace OrionPOS.Domain.Catalogo;

public sealed class TabelaPreco
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;  // "Padrão", "Happy Hour"
    public bool Ativo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public List<PrecoVariant> Precos { get; set; } = [];
}
