using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrionPOS.Infra.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_Catalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS menu_option CASCADE;
                DROP TABLE IF EXISTS menu_option_group CASCADE;
                DROP TABLE IF EXISTS menu_item CASCADE;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS app_user (
                    id uuid NOT NULL,
                    email text NOT NULL,
                    password_hash text NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_app_user" PRIMARY KEY (id)
                );
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_app_user_email" ON app_user (email);
                """);

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modifier_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    min_selecoes = table.Column<int>(type: "integer", nullable: false),
                    max_selecoes = table.Column<int>(type: "integer", nullable: true),
                    obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modifier_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_modifier_groups_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tabelas_preco",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tabelas_preco", x => x.id);
                    table.ForeignKey(
                        name: "FK_tabelas_preco_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: true),
                    tempo_preparo = table.Column<int>(type: "integer", nullable: false),
                    controla_estoque = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.id);
                    table.ForeignKey(
                        name: "FK_produtos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_produtos_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modifier_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modifier_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    preco_adicional = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modifier_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_modifier_options_modifier_groups_modifier_group_id",
                        column: x => x.modifier_group_id,
                        principalTable: "modifier_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cardapios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tabela_preco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cardapios", x => x.id);
                    table.ForeignKey(
                        name: "FK_cardapios_tabelas_preco_tabela_preco_id",
                        column: x => x.tabela_preco_id,
                        principalTable: "tabelas_preco",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cardapios_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_modifier_groups",
                columns: table => new
                {
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modifier_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_modifier_groups", x => new { x.produto_id, x.modifier_group_id });
                    table.ForeignKey(
                        name: "FK_produto_modifier_groups_modifier_groups_modifier_group_id",
                        column: x => x.modifier_group_id,
                        principalTable: "modifier_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_produto_modifier_groups_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_produto_variants_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cardapio_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cardapio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cardapio_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_cardapio_itens_cardapios_cardapio_id",
                        column: x => x.cardapio_id,
                        principalTable: "cardapios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cardapio_itens_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cardapio_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "precos_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tabela_preco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produto_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preco = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: false),
                    preco_promocional = table.Column<decimal>(type: "numeric(14,4)", precision: 14, scale: 4, nullable: true),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precos_variants", x => x.id);
                    table.ForeignKey(
                        name: "FK_precos_variants_produto_variants_produto_variant_id",
                        column: x => x.produto_variant_id,
                        principalTable: "produto_variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_precos_variants_tabelas_preco_tabela_preco_id",
                        column: x => x.tabela_preco_id,
                        principalTable: "tabelas_preco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_itens_cardapio_id_produto_id",
                table: "cardapio_itens",
                columns: new[] { "cardapio_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_itens_categoria_id",
                table: "cardapio_itens",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_itens_produto_id",
                table: "cardapio_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "IX_cardapios_tabela_preco_id",
                table: "cardapios",
                column: "tabela_preco_id");

            migrationBuilder.CreateIndex(
                name: "IX_cardapios_tenant_id",
                table: "cardapios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_tenant_id",
                table: "categorias",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_modifier_groups_tenant_id",
                table: "modifier_groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_modifier_options_modifier_group_id",
                table: "modifier_options",
                column: "modifier_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_precos_variants_produto_variant_id",
                table: "precos_variants",
                column: "produto_variant_id");

            migrationBuilder.CreateIndex(
                name: "IX_precos_variants_tabela_preco_id_produto_variant_id",
                table: "precos_variants",
                columns: new[] { "tabela_preco_id", "produto_variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_produto_modifier_groups_modifier_group_id",
                table: "produto_modifier_groups",
                column: "modifier_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_produto_variants_produto_id",
                table: "produto_variants",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_tenant_id",
                table: "produtos",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tabelas_preco_tenant_id",
                table: "tabelas_preco",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "cardapio_itens");

            migrationBuilder.DropTable(
                name: "modifier_options");

            migrationBuilder.DropTable(
                name: "precos_variants");

            migrationBuilder.DropTable(
                name: "produto_modifier_groups");

            migrationBuilder.DropTable(
                name: "cardapios");

            migrationBuilder.DropTable(
                name: "produto_variants");

            migrationBuilder.DropTable(
                name: "modifier_groups");

            migrationBuilder.DropTable(
                name: "tabelas_preco");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
