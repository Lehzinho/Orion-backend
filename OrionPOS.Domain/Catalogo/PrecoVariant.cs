namespace OrionPOS.Domain.Catalogo;

public sealed class PrecoVariant
{
    public Guid Id { get; set; }
    public Guid TabelaPrecoId { get; set; }
    public Guid ProdutoVariantId { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public TabelaPreco TabelaPreco { get; set; } = null!;
    public ProdutoVariant ProdutoVariant { get; set; } = null!;
}
