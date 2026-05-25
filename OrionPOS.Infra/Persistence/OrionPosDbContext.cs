using Microsoft.EntityFrameworkCore;
using OrionPOS.Domain.Auth;
using OrionPOS.Domain.Catalogo;

namespace OrionPOS.Infra.Persistence;

public sealed class OrionPosDbContext(DbContextOptions<OrionPosDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoVariant> ProdutoVariants => Set<ProdutoVariant>();
    public DbSet<TabelaPreco> TabelasPreco => Set<TabelaPreco>();
    public DbSet<PrecoVariant> PrecosVariants => Set<PrecoVariant>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<ProdutoModifierGroup> ProdutoModifierGroups => Set<ProdutoModifierGroup>();
    public DbSet<Cardapio> Cardapios => Set<Cardapio>();
    public DbSet<CardapioItem> CardapioItens => Set<CardapioItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("categorias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.HasOne(x => x.Tenant).WithMany()
                .HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.ToTable("produtos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Descricao).HasColumnName("descricao");
            e.Property(x => x.Tipo).HasColumnName("tipo")
                .HasConversion<string>();
            e.Property(x => x.Sku).HasColumnName("sku");
            e.Property(x => x.TempoPreparo).HasColumnName("tempo_preparo");
            e.Property(x => x.ControlaEstoque).HasColumnName("controla_estoque");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Tenant).WithMany()
                .HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.Categoria).WithMany(x => x.Produtos)
                .HasForeignKey(x => x.CategoriaId);
            e.HasMany(x => x.Variants).WithOne(x => x.Produto)
                .HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProdutoVariant>(e =>
        {
            e.ToTable("produto_variants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProdutoId).HasColumnName("produto_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Sku).HasColumnName("sku");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Ativo).HasColumnName("ativo");
        });

        modelBuilder.Entity<TabelaPreco>(e =>
        {
            e.ToTable("tabelas_preco");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.HasOne(x => x.Tenant).WithMany()
                .HasForeignKey(x => x.TenantId);
        });

        modelBuilder.Entity<PrecoVariant>(e =>
        {
            e.ToTable("precos_variants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TabelaPrecoId).HasColumnName("tabela_preco_id");
            e.Property(x => x.ProdutoVariantId).HasColumnName("produto_variant_id");
            e.Property(x => x.Preco).HasColumnName("preco").HasPrecision(14, 4);
            e.Property(x => x.PrecoPromocional).HasColumnName("preco_promocional").HasPrecision(14, 4);
            e.Property(x => x.DataInicio).HasColumnName("data_inicio");
            e.Property(x => x.DataFim).HasColumnName("data_fim");
            e.HasIndex(x => new { x.TabelaPrecoId, x.ProdutoVariantId }).IsUnique();
            e.HasOne(x => x.TabelaPreco).WithMany(x => x.Precos)
                .HasForeignKey(x => x.TabelaPrecoId);
            e.HasOne(x => x.ProdutoVariant).WithMany(x => x.Precos)
                .HasForeignKey(x => x.ProdutoVariantId);
        });

        modelBuilder.Entity<ModifierGroup>(e =>
        {
            e.ToTable("modifier_groups");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.MinSelecoes).HasColumnName("min_selecoes");
            e.Property(x => x.MaxSelecoes).HasColumnName("max_selecoes");
            e.Property(x => x.Obrigatorio).HasColumnName("obrigatorio");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.HasOne(x => x.Tenant).WithMany()
                .HasForeignKey(x => x.TenantId);
            e.HasMany(x => x.Opcoes).WithOne(x => x.ModifierGroup)
                .HasForeignKey(x => x.ModifierGroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ModifierOption>(e =>
        {
            e.ToTable("modifier_options");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ModifierGroupId).HasColumnName("modifier_group_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.PrecoAdicional).HasColumnName("preco_adicional").HasPrecision(14, 4);
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Ativo).HasColumnName("ativo");
        });

        modelBuilder.Entity<ProdutoModifierGroup>(e =>
        {
            e.ToTable("produto_modifier_groups");
            e.HasKey(x => new { x.ProdutoId, x.ModifierGroupId });
            e.Property(x => x.ProdutoId).HasColumnName("produto_id");
            e.Property(x => x.ModifierGroupId).HasColumnName("modifier_group_id");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.HasOne(x => x.Produto).WithMany(x => x.ModifierGroups)
                .HasForeignKey(x => x.ProdutoId);
            e.HasOne(x => x.ModifierGroup).WithMany(x => x.Produtos)
                .HasForeignKey(x => x.ModifierGroupId);
        });

        modelBuilder.Entity<Cardapio>(e =>
        {
            e.ToTable("cardapios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.TabelaPrecoId).HasColumnName("tabela_preco_id");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.HasOne(x => x.Tenant).WithMany()
                .HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.TabelaPreco).WithMany()
                .HasForeignKey(x => x.TabelaPrecoId);
            e.HasMany(x => x.Itens).WithOne(x => x.Cardapio)
                .HasForeignKey(x => x.CardapioId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CardapioItem>(e =>
        {
            e.ToTable("cardapio_itens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CardapioId).HasColumnName("cardapio_id");
            e.Property(x => x.ProdutoId).HasColumnName("produto_id");
            e.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            e.Property(x => x.Ordem).HasColumnName("ordem");
            e.Property(x => x.Ativo).HasColumnName("ativo");
            e.HasIndex(x => new { x.CardapioId, x.ProdutoId }).IsUnique();
            e.HasOne(x => x.Produto).WithMany()
                .HasForeignKey(x => x.ProdutoId);
            e.HasOne(x => x.Categoria).WithMany()
                .HasForeignKey(x => x.CategoriaId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("app_user");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}
