namespace OrionPOS.Domain.Catalogo;

public sealed class ModifierGroup
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Nome { get; set; } = string.Empty;  // "Adicionais", "Ponto da Carne"
    public int MinSelecoes { get; set; } = 0;
    public int? MaxSelecoes { get; set; }
    public bool Obrigatorio { get; set; } = false;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public List<ModifierOption> Opcoes { get; set; } = [];
    public List<ProdutoModifierGroup> Produtos { get; set; } = [];
}
