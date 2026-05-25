using Microsoft.EntityFrameworkCore;
using OrionPOS.Application.Catalogo;
using OrionPOS.Domain.Catalogo;
using OrionPOS.Infra.Persistence;

namespace OrionPOS.Infra.Repositories;

public sealed class CatalogRepository(OrionPosDbContext dbContext) : ICatalogRepository
{
    public async Task<CardapioDto?> GetCardapioPadraoAsync(Guid tenantId, CancellationToken ct)
    {
        var cardapio = await dbContext.Cardapios
            .AsSplitQuery()
            .Include(c => c.TabelaPreco)
            .Include(c => c.Itens.Where(i => i.Ativo))
                .ThenInclude(i => i.Produto!)
                    .ThenInclude(p => p!.Variants.Where(v => v.Ativo))
                        .ThenInclude(v => v.Precos)
            .Include(c => c.Itens.Where(i => i.Ativo))
                .ThenInclude(i => i.Categoria)
            .Include(c => c.Itens.Where(i => i.Ativo))
                .ThenInclude(i => i.Produto!)
                    .ThenInclude(p => p!.Categoria)
            .Include(c => c.Itens.Where(i => i.Ativo))
                .ThenInclude(i => i.Produto!)
                    .ThenInclude(p => p!.ModifierGroups)
                        .ThenInclude(pmg => pmg.ModifierGroup)
                            .ThenInclude(mg => mg.Opcoes.Where(o => o.Ativo))
            .OrderBy(c => c.Nome)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.Nome == "Padrão", ct);

        if (cardapio is null)
        {
            return null;
        }

        var tabelaPrecoId = cardapio.TabelaPrecoId;
        var now = DateTime.UtcNow;

        var itensOrdenados = cardapio.Itens
            .Where(i => i.Ativo && i.Produto is { Ativo: true })
            .OrderBy(i => i.Ordem)
            .ToList();

        var secoes = itensOrdenados
            .GroupBy(i => SecaoKey(i))
            .OrderBy(g => g.Min(i => i.Ordem))
            .Select(g => new CardapioSecaoDto(
                g.Key,
                g.OrderBy(i => i.Ordem).Select(i => MapProduto(i.Produto!, tabelaPrecoId, now)).ToList()))
            .ToList();

        return new CardapioDto(cardapio.Id, cardapio.Nome, secoes);
    }

    private static string SecaoKey(CardapioItem item)
    {
        if (item.Categoria is { Nome: { Length: > 0 } nomeItem })
        {
            return nomeItem;
        }

        if (item.Produto?.Categoria is { Nome: { Length: > 0 } nomeProd })
        {
            return nomeProd;
        }

        return "Outros";
    }

    private static ProdutoDto MapProduto(Produto p, Guid? tabelaPrecoId, DateTime now)
    {
        var variants = p.Variants
            .Where(v => v.Ativo)
            .OrderBy(v => v.Ordem)
            .Select(v => new ProdutoVariantDto(
                v.Id,
                v.Nome,
                ResolvePreco(v, tabelaPrecoId, now),
                v.Ordem))
            .ToList();

        var modifierGroups = p.ModifierGroups
            .OrderBy(pmg => pmg.Ordem)
            .Select(pmg => MapModifierGroup(pmg.ModifierGroup))
            .ToList();

        return new ProdutoDto(
            p.Id,
            p.Nome,
            p.Descricao,
            p.Tipo.ToString(),
            p.Categoria?.Nome,
            p.TempoPreparo,
            variants,
            modifierGroups);
    }

    private static decimal ResolvePreco(ProdutoVariant v, Guid? tabelaPrecoId, DateTime now)
    {
        if (tabelaPrecoId is null)
        {
            return 0;
        }

        var pv = v.Precos.FirstOrDefault(x => x.TabelaPrecoId == tabelaPrecoId.Value);
        if (pv is null)
        {
            return 0;
        }

        if (pv.PrecoPromocional is decimal promo
            && (pv.DataInicio is null || pv.DataInicio <= now)
            && (pv.DataFim is null || pv.DataFim >= now))
        {
            return promo;
        }

        return pv.Preco;
    }

    private static ModifierGroupDto MapModifierGroup(ModifierGroup mg)
    {
        var opcoes = mg.Opcoes
            .Where(o => o.Ativo)
            .OrderBy(o => o.Ordem)
            .Select(o => new ModifierOptionDto(o.Id, o.Nome, o.PrecoAdicional, o.Ordem))
            .ToList();

        return new ModifierGroupDto(
            mg.Id,
            mg.Nome,
            mg.MinSelecoes,
            mg.MaxSelecoes,
            mg.Obrigatorio,
            mg.Ordem,
            opcoes);
    }
}
