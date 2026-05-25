namespace OrionPOS.Application.Catalogo;

public record ModifierOptionDto(
    Guid Id,
    string Nome,
    decimal PrecoAdicional,
    int Ordem
);

public record ModifierGroupDto(
    Guid Id,
    string Nome,
    int MinSelecoes,
    int? MaxSelecoes,
    bool Obrigatorio,
    int Ordem,
    List<ModifierOptionDto> Opcoes
);

public record ProdutoVariantDto(
    Guid Id,
    string Nome,
    decimal Preco,  // preço da tabela padrão
    int Ordem
);

public record ProdutoDto(
    Guid Id,
    string Nome,
    string Descricao,
    string Tipo,
    string? Categoria,
    int TempoPreparo,
    List<ProdutoVariantDto> Variants,
    List<ModifierGroupDto> ModifierGroups
);

public record CardapioDto(
    Guid Id,
    string Nome,
    List<CardapioSecaoDto> Secoes
);

public record CardapioSecaoDto(
    string Categoria,
    List<ProdutoDto> Produtos
);
