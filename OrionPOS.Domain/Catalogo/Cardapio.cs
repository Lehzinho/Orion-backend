namespace OrionPOS.Domain.Catalogo;

public sealed class Cardapio
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid? TabelaPrecoId { get; set; }
    public bool Ativo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public TabelaPreco? TabelaPreco { get; set; }
    public List<CardapioItem> Itens { get; set; } = [];
}
