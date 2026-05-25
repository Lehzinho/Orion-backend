namespace OrionPOS.Domain.Catalogo;

public sealed class ProdutoVariant
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;  // "Grande", "500ml", "Padrão"
    public string? Sku { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public Produto Produto { get; set; } = null!;
    public List<PrecoVariant> Precos { get; set; } = [];
}
