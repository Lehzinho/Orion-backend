using Microsoft.EntityFrameworkCore;
using OrionPOS.Domain.Auth;
using OrionPOS.Domain.Catalogo;

namespace OrionPOS.Infra.Persistence;

public static class DbInitializer
{
    public static readonly Guid DevTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(OrionPosDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
        await EnsureAdminUserAsync(dbContext);

        if (!await dbContext.Tenants.AnyAsync(t => t.Id == DevTenantId))
        {
            await SeedInitialTenantAsync(dbContext);
        }

        await EnsureRichCatalogSeedAsync(dbContext);
    }

    private static async Task SeedInitialTenantAsync(OrionPosDbContext dbContext)
    {
        var now = DateTime.UtcNow;

        var tenant = new Tenant
        {
            Id = DevTenantId,
            Nome = "Orion Dev",
            Ativo = true,
            CreatedAt = now,
        };

        var tabelaPrecoId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var tabelaPreco = new TabelaPreco
        {
            Id = tabelaPrecoId,
            TenantId = DevTenantId,
            Nome = "Padrão",
            Ativo = true,
        };

        var catHambId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var catBebId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var catSobId = Guid.Parse("20000000-0000-0000-0000-000000000003");

        var categorias = new[]
        {
            new Categoria { Id = catHambId, TenantId = DevTenantId, Nome = "Hambúrgueres", Ordem = 1, Ativo = true },
            new Categoria { Id = catBebId, TenantId = DevTenantId, Nome = "Bebidas", Ordem = 2, Ativo = true },
            new Categoria { Id = catSobId, TenantId = DevTenantId, Nome = "Sobremesas", Ordem = 3, Ativo = true },
        };

        var mgAdicionaisId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var mgPontoId = Guid.Parse("30000000-0000-0000-0000-000000000002");

        var mgAdicionais = new ModifierGroup
        {
            Id = mgAdicionaisId,
            TenantId = DevTenantId,
            Nome = "Adicionais",
            MinSelecoes = 0,
            MaxSelecoes = 5,
            Obrigatorio = false,
            Ordem = 2,
            Ativo = true,
            Opcoes =
            [
                new ModifierOption
                {
                    Id = Guid.Parse("31000000-0000-0000-0000-000000000001"),
                    Nome = "Bacon",
                    PrecoAdicional = 3m,
                    Ordem = 1,
                    Ativo = true,
                },
                new ModifierOption
                {
                    Id = Guid.Parse("31000000-0000-0000-0000-000000000002"),
                    Nome = "Queijo extra",
                    PrecoAdicional = 2m,
                    Ordem = 2,
                    Ativo = true,
                },
                new ModifierOption
                {
                    Id = Guid.Parse("31000000-0000-0000-0000-000000000003"),
                    Nome = "Ovo frito",
                    PrecoAdicional = 2.5m,
                    Ordem = 3,
                    Ativo = true,
                },
            ],
        };

        var mgPonto = new ModifierGroup
        {
            Id = mgPontoId,
            TenantId = DevTenantId,
            Nome = "Ponto da Carne",
            MinSelecoes = 1,
            MaxSelecoes = 1,
            Obrigatorio = true,
            Ordem = 1,
            Ativo = true,
            Opcoes =
            [
                new ModifierOption
                {
                    Id = Guid.Parse("32000000-0000-0000-0000-000000000001"),
                    Nome = "Mal passado",
                    PrecoAdicional = 0m,
                    Ordem = 1,
                    Ativo = true,
                },
                new ModifierOption
                {
                    Id = Guid.Parse("32000000-0000-0000-0000-000000000002"),
                    Nome = "Ao ponto",
                    PrecoAdicional = 0m,
                    Ordem = 2,
                    Ativo = true,
                },
                new ModifierOption
                {
                    Id = Guid.Parse("32000000-0000-0000-0000-000000000003"),
                    Nome = "Bem passado",
                    PrecoAdicional = 0m,
                    Ordem = 3,
                    Ativo = true,
                },
            ],
        };

        var royalId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var royalVariantId = Guid.Parse("41000000-0000-0000-0000-000000000001");

        var royal = new Produto
        {
            Id = royalId,
            TenantId = DevTenantId,
            CategoriaId = catHambId,
            Nome = "Royal Burger",
            Descricao = "Blend 180 g, queijo, salada e molho especial.",
            Tipo = TipoProduto.Produto,
            TempoPreparo = 15,
            ControlaEstoque = false,
            Ativo = true,
            CreatedAt = now,
            Variants =
            [
                new ProdutoVariant
                {
                    Id = royalVariantId,
                    Nome = "Padrão",
                    Ordem = 1,
                    Ativo = true,
                },
            ],
            ModifierGroups =
            [
                new ProdutoModifierGroup { ProdutoId = royalId, ModifierGroupId = mgPontoId, Ordem = 1 },
                new ProdutoModifierGroup { ProdutoId = royalId, ModifierGroupId = mgAdicionaisId, Ordem = 2 },
            ],
        };

        var cocaId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        var cocaLataId = Guid.Parse("41000000-0000-0000-0000-000000000002");
        var coca2lId = Guid.Parse("41000000-0000-0000-0000-000000000003");

        var coca = new Produto
        {
            Id = cocaId,
            TenantId = DevTenantId,
            CategoriaId = catBebId,
            Nome = "Coca-Cola",
            Descricao = "Refrigerante gelado.",
            Tipo = TipoProduto.Produto,
            TempoPreparo = 2,
            ControlaEstoque = false,
            Ativo = true,
            CreatedAt = now,
            Variants =
            [
                new ProdutoVariant { Id = cocaLataId, Nome = "Lata", Ordem = 1, Ativo = true },
                new ProdutoVariant { Id = coca2lId, Nome = "2 Litros", Ordem = 2, Ativo = true },
            ],
        };

        var batataId = Guid.Parse("40000000-0000-0000-0000-000000000003");
        var batataPId = Guid.Parse("41000000-0000-0000-0000-000000000004");
        var batataGId = Guid.Parse("41000000-0000-0000-0000-000000000005");

        var batata = new Produto
        {
            Id = batataId,
            TenantId = DevTenantId,
            CategoriaId = catSobId,
            Nome = "Batata Frita",
            Descricao = "Batatas crocantes com sal.",
            Tipo = TipoProduto.Produto,
            TempoPreparo = 10,
            ControlaEstoque = false,
            Ativo = true,
            CreatedAt = now,
            Variants =
            [
                new ProdutoVariant { Id = batataPId, Nome = "Pequena", Ordem = 1, Ativo = true },
                new ProdutoVariant { Id = batataGId, Nome = "Grande", Ordem = 2, Ativo = true },
            ],
        };

        var precos = new[]
        {
            new PrecoVariant
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                TabelaPrecoId = tabelaPrecoId,
                ProdutoVariantId = royalVariantId,
                Preco = 28.90m,
            },
            new PrecoVariant
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                TabelaPrecoId = tabelaPrecoId,
                ProdutoVariantId = cocaLataId,
                Preco = 6m,
            },
            new PrecoVariant
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                TabelaPrecoId = tabelaPrecoId,
                ProdutoVariantId = coca2lId,
                Preco = 12m,
            },
            new PrecoVariant
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                TabelaPrecoId = tabelaPrecoId,
                ProdutoVariantId = batataPId,
                Preco = 12m,
            },
            new PrecoVariant
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                TabelaPrecoId = tabelaPrecoId,
                ProdutoVariantId = batataGId,
                Preco = 18m,
            },
        };

        var cardapioId = Guid.Parse("60000000-0000-0000-0000-000000000001");
        var cardapio = new Cardapio
        {
            Id = cardapioId,
            TenantId = DevTenantId,
            Nome = "Padrão",
            TabelaPrecoId = tabelaPrecoId,
            Ativo = true,
            Itens =
            [
                new CardapioItem
                {
                    Id = Guid.Parse("61000000-0000-0000-0000-000000000001"),
                    ProdutoId = royalId,
                    CategoriaId = catHambId,
                    Ordem = 1,
                    Ativo = true,
                },
                new CardapioItem
                {
                    Id = Guid.Parse("61000000-0000-0000-0000-000000000002"),
                    ProdutoId = cocaId,
                    CategoriaId = catBebId,
                    Ordem = 2,
                    Ativo = true,
                },
                new CardapioItem
                {
                    Id = Guid.Parse("61000000-0000-0000-0000-000000000003"),
                    ProdutoId = batataId,
                    CategoriaId = catSobId,
                    Ordem = 3,
                    Ativo = true,
                },
            ],
        };

        dbContext.Tenants.Add(tenant);
        dbContext.TabelasPreco.Add(tabelaPreco);
        dbContext.Categorias.AddRange(categorias);
        dbContext.ModifierGroups.AddRange(mgAdicionais, mgPonto);
        dbContext.Produtos.AddRange(royal, coca, batata);
        dbContext.PrecosVariants.AddRange(precos);
        dbContext.Cardapios.Add(cardapio);

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 22 produtos adicionais com variantes, modifiers e preços variados (inclui algumas promoções).
    /// Referências gerais: porções tipo sports bar, pizzas estilo cadeias EUA, bowls, cafés e grelhados.
    /// </summary>
    private static async Task EnsureRichCatalogSeedAsync(OrionPosDbContext dbContext)
    {
        if (!await dbContext.Tenants.AnyAsync(t => t.Id == DevTenantId))
        {
            return;
        }

        var richMarker = Guid.Parse("70100000-0000-0000-0000-000000000001");
        if (await dbContext.Produtos.AnyAsync(p => p.Id == richMarker))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var tabelaPadraoId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var cardapioPadraoId = Guid.Parse("60000000-0000-0000-0000-000000000001");

        var mgAdicionaisId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var mgPontoId = Guid.Parse("30000000-0000-0000-0000-000000000002");

        static Guid Pid(int n) => Guid.Parse($"70100000-0000-0000-0000-{n:D12}");
        static Guid Vid(int n) => Guid.Parse($"70200000-0000-0000-0000-{n:D12}");
        static Guid Prid(int n) => Guid.Parse($"70300000-0000-0000-0000-{n:D12}");

        var catEntradas = Guid.Parse("20000000-0000-0000-0000-000000000004");
        var catPizzas = Guid.Parse("20000000-0000-0000-0000-000000000005");
        var catBowls = Guid.Parse("20000000-0000-0000-0000-000000000006");
        var catCafes = Guid.Parse("20000000-0000-0000-0000-000000000007");
        var catGrelha = Guid.Parse("20000000-0000-0000-0000-000000000008");
        var catHambId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var catBebId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var catSobId = Guid.Parse("20000000-0000-0000-0000-000000000003");

        dbContext.Categorias.AddRange(
            new Categoria { Id = catEntradas, TenantId = DevTenantId, Nome = "Entradas & Porções", Ordem = 4, Ativo = true },
            new Categoria { Id = catPizzas, TenantId = DevTenantId, Nome = "Pizzas", Ordem = 5, Ativo = true },
            new Categoria { Id = catBowls, TenantId = DevTenantId, Nome = "Saladas & Bowls", Ordem = 6, Ativo = true },
            new Categoria { Id = catCafes, TenantId = DevTenantId, Nome = "Cafés & Chás", Ordem = 7, Ativo = true },
            new Categoria { Id = catGrelha, TenantId = DevTenantId, Nome = "Grelhados", Ordem = 8, Ativo = true });

        var mgTemperoAsas = Guid.Parse("70400000-0000-0000-0000-000000000001");
        var mgPizzaExtra = Guid.Parse("70400000-0000-0000-0000-000000000002");
        var mgMolhoSalada = Guid.Parse("70400000-0000-0000-0000-000000000003");
        var mgLeiteCafe = Guid.Parse("70400000-0000-0000-0000-000000000004");
        var mgExtrasNachos = Guid.Parse("70400000-0000-0000-0000-000000000005");
        var mgExtraProteina = Guid.Parse("70400000-0000-0000-0000-000000000006");
        var mgBrownie = Guid.Parse("70400000-0000-0000-0000-000000000007");

        dbContext.ModifierGroups.AddRange(
            new ModifierGroup
            {
                Id = mgTemperoAsas,
                TenantId = DevTenantId,
                Nome = "Tempero das asas",
                MinSelecoes = 1,
                MaxSelecoes = 1,
                Obrigatorio = true,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000001"), Nome = "Mild", PrecoAdicional = 0, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000002"), Nome = "Hot Buffalo", PrecoAdicional = 0, Ordem = 2, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000003"), Nome = "BBQ defumado", PrecoAdicional = 1m, Ordem = 3, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000004"), Nome = "Lemon pepper", PrecoAdicional = 1.5m, Ordem = 4, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgPizzaExtra,
                TenantId = DevTenantId,
                Nome = "Acabamento da pizza",
                MinSelecoes = 0,
                MaxSelecoes = 2,
                Obrigatorio = false,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000005"), Nome = "Borda com alho", PrecoAdicional = 0, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000006"), Nome = "Miel de mostarda", PrecoAdicional = 3m, Ordem = 2, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000007"), Nome = "Drizzle de trufa", PrecoAdicional = 6m, Ordem = 3, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000008"), Nome = "Raspas de limão", PrecoAdicional = 0, Ordem = 4, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgMolhoSalada,
                TenantId = DevTenantId,
                Nome = "Molho / dressing",
                MinSelecoes = 1,
                MaxSelecoes = 1,
                Obrigatorio = true,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000009"), Nome = "Caesar", PrecoAdicional = 0, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000010"), Nome = "Ranch", PrecoAdicional = 0, Ordem = 2, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000011"), Nome = "Balsâmico", PrecoAdicional = 0, Ordem = 3, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000012"), Nome = "Tahine com limão", PrecoAdicional = 2m, Ordem = 4, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgLeiteCafe,
                TenantId = DevTenantId,
                Nome = "Leite / vegetal",
                MinSelecoes = 1,
                MaxSelecoes = 1,
                Obrigatorio = true,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000013"), Nome = "Integral", PrecoAdicional = 0, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000014"), Nome = "Desnatado", PrecoAdicional = 0, Ordem = 2, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000015"), Nome = "Aveia", PrecoAdicional = 2m, Ordem = 3, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000016"), Nome = "Sem lactose", PrecoAdicional = 1.5m, Ordem = 4, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgExtrasNachos,
                TenantId = DevTenantId,
                Nome = "Extras nachos",
                MinSelecoes = 0,
                MaxSelecoes = 3,
                Obrigatorio = false,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000017"), Nome = "Guacamole", PrecoAdicional = 4m, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000018"), Nome = "Jalapeño extra", PrecoAdicional = 2m, Ordem = 2, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000019"), Nome = "Chili com carne", PrecoAdicional = 5m, Ordem = 3, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgExtraProteina,
                TenantId = DevTenantId,
                Nome = "Proteína extra no bowl",
                MinSelecoes = 0,
                MaxSelecoes = 1,
                Obrigatorio = false,
                Ordem = 2,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000020"), Nome = "Frango grelhado +120 g", PrecoAdicional = 8m, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000021"), Nome = "Tofu marinado", PrecoAdicional = 6m, Ordem = 2, Ativo = true },
                ],
            },
            new ModifierGroup
            {
                Id = mgBrownie,
                TenantId = DevTenantId,
                Nome = "Extras brownie",
                MinSelecoes = 0,
                MaxSelecoes = 1,
                Obrigatorio = false,
                Ordem = 1,
                Ativo = true,
                Opcoes =
                [
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000022"), Nome = "Calda quente chocolate", PrecoAdicional = 3m, Ordem = 1, Ativo = true },
                    new ModifierOption { Id = Guid.Parse("70500000-0000-0000-0000-000000000023"), Nome = "Castanha caramelizada", PrecoAdicional = 4m, Ordem = 2, Ativo = true },
                ],
            });

        var promoIni = now.AddDays(-2);
        var promoFim = now.AddYears(3);

        static PrecoVariant Px(Guid id, Guid tabela, Guid variant, decimal preco, decimal? promo, DateTime? ini, DateTime? fim) => new()
        {
            Id = id,
            TabelaPrecoId = tabela,
            ProdutoVariantId = variant,
            Preco = preco,
            PrecoPromocional = promo,
            DataInicio = ini,
            DataFim = fim,
        };

        var novosProdutos = new List<Produto>
        {
            new()
            {
                Id = Pid(1), TenantId = DevTenantId, CategoriaId = catEntradas, Nome = "Asas Buffalo",
                Descricao = "Porção crocante; molhos estilo sports bar.", Tipo = TipoProduto.Produto, TempoPreparo = 14, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(1), Nome = "8 unidades", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(2), Nome = "12 unidades", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(1), ModifierGroupId = mgTemperoAsas, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(2), TenantId = DevTenantId, CategoriaId = catEntradas, Nome = "Anéis de cebola empanados",
                Descricao = "Servidos com molho ranch.", Tipo = TipoProduto.Produto, TempoPreparo = 10, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(3), Nome = "Porção P", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(4), Nome = "Porção M", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(5), Nome = "Porção G", Ordem = 3, Ativo = true },
                ],
            },
            new()
            {
                Id = Pid(3), TenantId = DevTenantId, CategoriaId = catEntradas, Nome = "Nachos da casa",
                Descricao = "Totopos com queijo, pico de gallo e extras à escolha.", Tipo = TipoProduto.Produto, TempoPreparo = 11, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(6), Nome = "Individual", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(7), Nome = "Para dividir (2 pessoas)", Ordem = 2, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(3), ModifierGroupId = mgExtrasNachos, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(4), TenantId = DevTenantId, CategoriaId = catPizzas, Nome = "Pizza Margherita",
                Descricao = "Mozzarella, tomate e manjericão.", Tipo = TipoProduto.Produto, TempoPreparo = 22, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(8), Nome = "Broto", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(9), Nome = "Média", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(10), Nome = "Grande", Ordem = 3, Ativo = true },
                    new ProdutoVariant { Id = Vid(11), Nome = "Família", Ordem = 4, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(4), ModifierGroupId = mgPizzaExtra, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(5), TenantId = DevTenantId, CategoriaId = catPizzas, Nome = "Pizza Pepperoni",
                Descricao = "Clássica pepperoni com mozzarella.", Tipo = TipoProduto.Produto, TempoPreparo = 22, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(12), Nome = "Broto", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(13), Nome = "Média", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(14), Nome = "Grande", Ordem = 3, Ativo = true },
                    new ProdutoVariant { Id = Vid(15), Nome = "Família", Ordem = 4, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(5), ModifierGroupId = mgPizzaExtra, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(6), TenantId = DevTenantId, CategoriaId = catPizzas, Nome = "Pizza BBQ Chicken",
                Descricao = "Frango desfiado com molho barbecue e cebola roxa.", Tipo = TipoProduto.Produto, TempoPreparo = 24, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(16), Nome = "Broto", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(17), Nome = "Média", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(18), Nome = "Grande", Ordem = 3, Ativo = true },
                    new ProdutoVariant { Id = Vid(19), Nome = "Família", Ordem = 4, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(6), ModifierGroupId = mgPizzaExtra, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(7), TenantId = DevTenantId, CategoriaId = catBowls, Nome = "Bowl Mediterrâneo",
                Descricao = "Grão-de-bico, pepino, tomate, feta e greens.", Tipo = TipoProduto.Produto, TempoPreparo = 12, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(20), Nome = "Regular", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(21), Nome = "Grande", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(7), ModifierGroupId = mgMolhoSalada, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(8), TenantId = DevTenantId, CategoriaId = catBowls, Nome = "Bowl Frango Teriyaki",
                Descricao = "Arroz, legumes salteados e frango com teriyaki.", Tipo = TipoProduto.Produto, TempoPreparo = 16, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(22), Nome = "Regular", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(23), Nome = "Grande", Ordem = 2, Ativo = true }],
                ModifierGroups =
                [
                    new ProdutoModifierGroup { ProdutoId = Pid(8), ModifierGroupId = mgMolhoSalada, Ordem = 1 },
                    new ProdutoModifierGroup { ProdutoId = Pid(8), ModifierGroupId = mgExtraProteina, Ordem = 2 },
                ],
            },
            new()
            {
                Id = Pid(9), TenantId = DevTenantId, CategoriaId = catBowls, Nome = "Caesar Salad",
                Descricao = "Clássica ou com tiras de frango grelhado.", Tipo = TipoProduto.Produto, TempoPreparo = 10, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(24), Nome = "Vegetariana", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(25), Nome = "Com frango", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(9), ModifierGroupId = mgMolhoSalada, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(10), TenantId = DevTenantId, CategoriaId = catHambId, Nome = "Smash Burger",
                Descricao = "Dois ou um disco smash com cheddar americano.", Tipo = TipoProduto.Produto, TempoPreparo = 14, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(26), Nome = "Simples (1 disco)", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(27), Nome = "Duplo (2 discos)", Ordem = 2, Ativo = true }],
                ModifierGroups =
                [
                    new ProdutoModifierGroup { ProdutoId = Pid(10), ModifierGroupId = mgPontoId, Ordem = 1 },
                    new ProdutoModifierGroup { ProdutoId = Pid(10), ModifierGroupId = mgAdicionaisId, Ordem = 2 },
                ],
            },
            new()
            {
                Id = Pid(11), TenantId = DevTenantId, CategoriaId = catHambId, Nome = "Mushroom Swiss Burger",
                Descricao = "Cogumelos salteados e queijo suíço.", Tipo = TipoProduto.Produto, TempoPreparo = 16, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(28), Nome = "Simples", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(29), Nome = "Duplo", Ordem = 2, Ativo = true }],
                ModifierGroups =
                [
                    new ProdutoModifierGroup { ProdutoId = Pid(11), ModifierGroupId = mgPontoId, Ordem = 1 },
                    new ProdutoModifierGroup { ProdutoId = Pid(11), ModifierGroupId = mgAdicionaisId, Ordem = 2 },
                ],
            },
            new()
            {
                Id = Pid(12), TenantId = DevTenantId, CategoriaId = catHambId, Nome = "Chicken Crispy Sandwich",
                Descricao = "Versão clássica ou picante.", Tipo = TipoProduto.Produto, TempoPreparo = 13, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(30), Nome = "Clássico", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(31), Nome = "Picante", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(12), ModifierGroupId = mgAdicionaisId, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(13), TenantId = DevTenantId, CategoriaId = catGrelha, Nome = "Picanha na chapa",
                Descricao = "Cortes com acompanamento do dia.", Tipo = TipoProduto.Produto, TempoPreparo = 26, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(32), Nome = "300 g", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(33), Nome = "400 g", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(34), Nome = "500 g", Ordem = 3, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(13), ModifierGroupId = mgPontoId, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(14), TenantId = DevTenantId, CategoriaId = catGrelha, Nome = "Salmão grelhado",
                Descricao = "Filete com ervas e limão.", Tipo = TipoProduto.Produto, TempoPreparo = 20, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(35), Nome = "200 g", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(36), Nome = "250 g", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(14), ModifierGroupId = mgPontoId, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(15), TenantId = DevTenantId, CategoriaId = catCafes, Nome = "Espresso",
                Descricao = "Café curto ou dose dupla.", Tipo = TipoProduto.Produto, TempoPreparo = 3, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(37), Nome = "Simples", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(38), Nome = "Doppio", Ordem = 2, Ativo = true }],
            },
            new()
            {
                Id = Pid(16), TenantId = DevTenantId, CategoriaId = catCafes, Nome = "Cappuccino",
                Descricao = "P, M ou G com leite à escolha.", Tipo = TipoProduto.Produto, TempoPreparo = 5, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(39), Nome = "Pequeno 220 ml", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(40), Nome = "Médio 350 ml", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(41), Nome = "Grande 470 ml", Ordem = 3, Ativo = true },
                ],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(16), ModifierGroupId = mgLeiteCafe, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(17), TenantId = DevTenantId, CategoriaId = catCafes, Nome = "Chai Latte",
                Descricao = "Especiarias com leite vaporizado.", Tipo = TipoProduto.Produto, TempoPreparo = 6, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(42), Nome = "12 oz", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(43), Nome = "16 oz", Ordem = 2, Ativo = true }],
            },
            new()
            {
                Id = Pid(18), TenantId = DevTenantId, CategoriaId = catBebId, Nome = "Água mineral",
                Descricao = "Com ou sem gás.", Tipo = TipoProduto.Produto, TempoPreparo = 1, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(44), Nome = "Sem gás 500 ml", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(45), Nome = "Com gás 500 ml", Ordem = 2, Ativo = true }],
            },
            new()
            {
                Id = Pid(19), TenantId = DevTenantId, CategoriaId = catBebId, Nome = "Suco natural laranja",
                Descricao = "Espremido na hora.", Tipo = TipoProduto.Produto, TempoPreparo = 4, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(46), Nome = "300 ml", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(47), Nome = "500 ml", Ordem = 2, Ativo = true }],
            },
            new()
            {
                Id = Pid(20), TenantId = DevTenantId, CategoriaId = catSobId, Nome = "Milk-shake",
                Descricao = "Baunilha, chocolate ou morango (opções no PDV futuro).", Tipo = TipoProduto.Produto, TempoPreparo = 5, Ativo = true, CreatedAt = now,
                Variants =
                [
                    new ProdutoVariant { Id = Vid(48), Nome = "300 ml", Ordem = 1, Ativo = true },
                    new ProdutoVariant { Id = Vid(49), Nome = "400 ml", Ordem = 2, Ativo = true },
                    new ProdutoVariant { Id = Vid(50), Nome = "500 ml", Ordem = 3, Ativo = true },
                ],
            },
            new()
            {
                Id = Pid(21), TenantId = DevTenantId, CategoriaId = catSobId, Nome = "Brownie com sorvete",
                Descricao = "Brownie quente com bola de sorvete.", Tipo = TipoProduto.Produto, TempoPreparo = 8, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(51), Nome = "1 bola", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(52), Nome = "2 bolas", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(21), ModifierGroupId = mgBrownie, Ordem = 1 }],
            },
            new()
            {
                Id = Pid(22), TenantId = DevTenantId, CategoriaId = catEntradas, Nome = "Quesadilla de frango",
                Descricao = "Tortilla grelhada com frango e queijo.", Tipo = TipoProduto.Produto, TempoPreparo = 12, Ativo = true, CreatedAt = now,
                Variants = [new ProdutoVariant { Id = Vid(53), Nome = "Regular", Ordem = 1, Ativo = true }, new ProdutoVariant { Id = Vid(54), Nome = "Grande", Ordem = 2, Ativo = true }],
                ModifierGroups = [new ProdutoModifierGroup { ProdutoId = Pid(22), ModifierGroupId = mgAdicionaisId, Ordem = 1 }],
            },
        };

        var precosNovos = new List<PrecoVariant>
        {
            Px(Prid(100), tabelaPadraoId, Vid(1), 31.90m, null, null, null),
            Px(Prid(101), tabelaPadraoId, Vid(2), 44.90m, null, null, null),
            Px(Prid(102), tabelaPadraoId, Vid(3), 17.50m, null, null, null),
            Px(Prid(103), tabelaPadraoId, Vid(4), 23.90m, null, null, null),
            Px(Prid(104), tabelaPadraoId, Vid(5), 28.50m, null, null, null),
            Px(Prid(105), tabelaPadraoId, Vid(6), 25.90m, null, null, null),
            Px(Prid(106), tabelaPadraoId, Vid(7), 41.90m, null, null, null),
            Px(Prid(107), tabelaPadraoId, Vid(8), 36.90m, null, null, null),
            Px(Prid(108), tabelaPadraoId, Vid(9), 46.90m, 42.90m, promoIni, promoFim),
            Px(Prid(109), tabelaPadraoId, Vid(10), 56.90m, null, null, null),
            Px(Prid(110), tabelaPadraoId, Vid(11), 71.90m, 64.90m, promoIni, promoFim),
            Px(Prid(111), tabelaPadraoId, Vid(12), 40.90m, null, null, null),
            Px(Prid(112), tabelaPadraoId, Vid(13), 50.90m, 47.90m, promoIni, promoFim),
            Px(Prid(113), tabelaPadraoId, Vid(14), 62.90m, null, null, null),
            Px(Prid(114), tabelaPadraoId, Vid(15), 77.90m, null, null, null),
            Px(Prid(115), tabelaPadraoId, Vid(16), 42.90m, null, null, null),
            Px(Prid(116), tabelaPadraoId, Vid(17), 53.90m, null, null, null),
            Px(Prid(117), tabelaPadraoId, Vid(18), 66.90m, null, null, null),
            Px(Prid(118), tabelaPadraoId, Vid(19), 81.90m, null, null, null),
            Px(Prid(119), tabelaPadraoId, Vid(20), 33.90m, null, null, null),
            Px(Prid(120), tabelaPadraoId, Vid(21), 41.50m, null, null, null),
            Px(Prid(121), tabelaPadraoId, Vid(22), 35.90m, null, null, null),
            Px(Prid(122), tabelaPadraoId, Vid(23), 45.90m, null, null, null),
            Px(Prid(123), tabelaPadraoId, Vid(24), 27.90m, null, null, null),
            Px(Prid(124), tabelaPadraoId, Vid(25), 37.90m, null, null, null),
            Px(Prid(125), tabelaPadraoId, Vid(26), 31.90m, null, null, null),
            Px(Prid(126), tabelaPadraoId, Vid(27), 41.90m, null, null, null),
            Px(Prid(127), tabelaPadraoId, Vid(28), 34.50m, null, null, null),
            Px(Prid(128), tabelaPadraoId, Vid(29), 45.50m, null, null, null),
            Px(Prid(129), tabelaPadraoId, Vid(30), 28.90m, null, null, null),
            Px(Prid(130), tabelaPadraoId, Vid(31), 30.90m, null, null, null),
            Px(Prid(131), tabelaPadraoId, Vid(32), 88.90m, null, null, null),
            Px(Prid(132), tabelaPadraoId, Vid(33), 108.90m, null, null, null),
            Px(Prid(133), tabelaPadraoId, Vid(34), 128.90m, null, null, null),
            Px(Prid(134), tabelaPadraoId, Vid(35), 53.90m, null, null, null),
            Px(Prid(135), tabelaPadraoId, Vid(36), 63.90m, null, null, null),
            Px(Prid(136), tabelaPadraoId, Vid(37), 7.90m, null, null, null),
            Px(Prid(137), tabelaPadraoId, Vid(38), 11.90m, null, null, null),
            Px(Prid(138), tabelaPadraoId, Vid(39), 13.90m, null, null, null),
            Px(Prid(139), tabelaPadraoId, Vid(40), 16.90m, null, null, null),
            Px(Prid(140), tabelaPadraoId, Vid(41), 20.90m, null, null, null),
            Px(Prid(141), tabelaPadraoId, Vid(42), 15.90m, null, null, null),
            Px(Prid(142), tabelaPadraoId, Vid(43), 18.90m, null, null, null),
            Px(Prid(143), tabelaPadraoId, Vid(44), 3.90m, null, null, null),
            Px(Prid(144), tabelaPadraoId, Vid(45), 4.50m, null, null, null),
            Px(Prid(145), tabelaPadraoId, Vid(46), 8.90m, 7.50m, promoIni, promoFim),
            Px(Prid(146), tabelaPadraoId, Vid(47), 13.90m, null, null, null),
            Px(Prid(147), tabelaPadraoId, Vid(48), 17.90m, null, null, null),
            Px(Prid(148), tabelaPadraoId, Vid(49), 21.90m, 18.90m, promoIni, promoFim),
            Px(Prid(149), tabelaPadraoId, Vid(50), 25.90m, null, null, null),
            Px(Prid(150), tabelaPadraoId, Vid(51), 22.90m, null, null, null),
            Px(Prid(151), tabelaPadraoId, Vid(52), 28.90m, null, null, null),
            Px(Prid(152), tabelaPadraoId, Vid(53), 25.90m, null, null, null),
            Px(Prid(153), tabelaPadraoId, Vid(54), 33.90m, null, null, null),
        };

        var itensCardapio = new List<CardapioItem>();
        var ordem = 4;
        for (var i = 1; i <= 22; i++)
        {
            itensCardapio.Add(new CardapioItem
            {
                Id = Guid.Parse($"70600000-0000-0000-0000-{i:D12}"),
                CardapioId = cardapioPadraoId,
                ProdutoId = Pid(i),
                CategoriaId = novosProdutos[i - 1].CategoriaId,
                Ordem = ordem++,
                Ativo = true,
            });
        }

        dbContext.Produtos.AddRange(novosProdutos);
        dbContext.PrecosVariants.AddRange(precosNovos);
        dbContext.CardapioItens.AddRange(itensCardapio);

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureAdminUserAsync(OrionPosDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@orion.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
            CreatedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();
    }
}
