namespace OrionPOS.Domain.Catalogo;

public sealed class Produto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoProduto Tipo { get; set; } = TipoProduto.Produto;
    public string? Sku { get; set; }
    public int TempoPreparo { get; set; }  // em minutos
    public bool ControlaEstoque { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Categoria? Categoria { get; set; }
    public List<ProdutoVariant> Variants { get; set; } = [];
    public List<ProdutoModifierGroup> ModifierGroups { get; set; } = [];
}
