namespace OrionPOS.Domain.Catalogo;

public sealed class ProdutoModifierGroup
{
    public Guid ProdutoId { get; set; }
    public Guid ModifierGroupId { get; set; }
    public int Ordem { get; set; }

    public Produto Produto { get; set; } = null!;
    public ModifierGroup ModifierGroup { get; set; } = null!;
}
