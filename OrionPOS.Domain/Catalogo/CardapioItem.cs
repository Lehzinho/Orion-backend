namespace OrionPOS.Domain.Catalogo;

public sealed class CardapioItem
{
    public Guid Id { get; set; }
    public Guid CardapioId { get; set; }
    public Guid ProdutoId { get; set; }
    public Guid? CategoriaId { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public Cardapio Cardapio { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
    public Categoria? Categoria { get; set; }
}
