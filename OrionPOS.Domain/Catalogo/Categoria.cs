namespace OrionPOS.Domain.Catalogo;

public sealed class Categoria
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public List<Produto> Produtos { get; set; } = [];
}
