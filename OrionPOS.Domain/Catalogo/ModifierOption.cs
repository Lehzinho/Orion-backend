namespace OrionPOS.Domain.Catalogo;

public sealed class ModifierOption
{
    public Guid Id { get; set; }
    public Guid ModifierGroupId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoAdicional { get; set; } = 0;
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;

    public ModifierGroup ModifierGroup { get; set; } = null!;
}
