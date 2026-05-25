# Migração ERP Delphi → C# (OrionERP)

**Projeto:** Reescrita do ERP legado em Delphi para C# .NET 9
**Status:** Em andamento
**Atualizado em:** 2026-03-26

---

## Sobre o Projeto

O **OrionERP** é um ERP multi-tenant voltado inicialmente para restaurantes, sendo desenvolvido do zero em C# como substituto de um sistema legado em Delphi/Pascal.

**Stack alvo:** C# .NET 9 · EF Core · PostgreSQL · Clean Architecture · CQRS · MediatR
**Inspiração:** modelo híbrido Toast POS / iFood / Square adaptado para ERP multi-tenant
**Foco do MVP:** PDV + produção + estoque + ticket cozinha

---

## Estrutura de Pastas

```
Migração ERP Delphi-CSharp/
├── README.md                        # Este arquivo
├── Orion-ERP.md                     # Fonte oficial de verdade do projeto
├── Claude Instructions.md           # Instruções para o Claude trabalhar no projeto
│
├── Módulos/                         # Documentação técnica por módulo
│   │
│   ├── — Arquitetura & Infraestrutura —
│   ├── Arquitetura Geral.md         # Clean Architecture, stack, estrutura de camadas
│   ├── Multi-Tenancy.md             # Isolamento por subdomínio, TenantProvider
│   ├── CQRS e MediatR.md            # Commands, Queries, Handlers, pipeline
│   ├── ValidationBehavior.md        # Pipeline de validação FluentValidation
│   ├── Repository Genérico.md       # IRepository<T>, paginação e filtros dinâmicos
│   ├── BaseCrudController.md        # Controller genérico CRUD
│   ├── ObjectMapper.md              # Mapeamento por expression trees sem AutoMapper
│   ├── OrionDbContext.md            # DbContext, migrations, auditoria automática
│   ├── EntityBase e Auditoria.md    # Classe base de entidades, campos auditáveis
│   ├── Exception Handling.md        # Middleware centralizado de tratamento de erros
│   ├── Logging e Observabilidade.md # Serilog estruturado
│   ├── Seed de Dados.md             # Dados iniciais de desenvolvimento
│   │
│   ├── — Módulos de Domínio (Implementados ✅) —
│   ├── Autenticação JWT.md          # Login, Refresh Token, Revogação
│   ├── Módulo Tenant.md             # Raiz do sistema, escopo global
│   ├── Módulo Empresa.md            # Unidade de negócio por tenant
│   ├── Módulo Usuário.md            # Usuários tenant-scoped
│   ├── Módulo EmpresaUsuario.md     # Vínculo N:N com papéis
│   ├── Módulo Departamento.md       # Divisões organizacionais (módulo de referência)
│   ├── Módulo Centro de Custo.md    # Agrupamento financeiro
│   ├── Módulo Almoxarifado.md       # Locais de armazenamento
│   ├── Módulo Tipo de Almoxarifado.md # Catálogo global de tipos
│   ├── Módulo Grupo de Preparo.md   # Agrupamentos de produção (restaurante)
│   ├── Módulo Ponto de Entrega.md   # Locais de entrega de pedidos
│   │
│   ├── — Módulos Parciais / Pendentes —
│   ├── Módulo PDV.md                # Entidade e EF prontos; CRUD pendente ⚠️
│   │
│   └── — Qualidade —
│       └── Testes.md                # Estratégia, cobertura atual e pendências
│
├── Research/                        # Materiais de referência e especificações
│   └── Orion-ERP/
│       ├── cartoes-internos/        # Cartões internos do sistema
│       ├── movimentos-caixas-turnos/ # Especificação: Movimento / Turno / Caixa
│       └── produtos/                # Especificação: Domínio de Produtos
│
└── 03_Resources/                    # Recursos técnicos e contexto para o Claude
    ├── CSharp DotNET.md
    ├── Arquitetura de Software.md
    └── claude/
        ├── system-prompt.md
        ├── decisions.md
        ├── agents/                  # Agentes especializados
        │   ├── cqrs-agent.md
        │   ├── efcore-agent.md
        │   ├── testing-agent.md
        │   └── obisidian-agent.md
        ├── context/                 # Contexto permanente de arquitetura
        │   ├── architecture.md
        │   ├── multi-tenancy.md
        │   └── patterns.md
        └── rules/
            └── implementation-rules.md
```

---

## Estado Atual

| Aspecto | Status |
|---|---|
| Arquitetura | Clean Architecture - 4 Camadas ✅ |
| Banco de Dados | EF Core + PostgreSQL ✅ |
| Autenticação | JWT + Refresh Token ✅ |
| Logging | Serilog (console + arquivo) ✅ |
| Multi-tenancy | Host-based + Slug Strategy ✅ |
| Exception Handling | Middleware Centralizado ✅ |
| API Documentation | Swagger/OpenAPI ✅ |
| Repositórios | IRepository\<T\> genérico ✅ |
| CQRS / MediatR | Commands + Queries implementados ✅ |
| Endpoints de Negócio | Auth, Empresa, Usuário, Departamento, CentroCusto, Almoxarifado, TipoAlmoxarifado, GrupoPreparo, PontoEntrega ✅ |
| Projeto de Testes | Implementado ✅ |
| PDV CRUD | Pendente ⚠️ |

---

## Próximos Passos

- [ ] PDV — CRUD (entidade e configuração EF prontas)
- [ ] Movimento / Turno / Caixa ← especificação pronta em `Research/`
- [ ] Produtos & Cardápio ← especificação pronta em `Research/`
- [ ] Pedidos / Comanda / Ticket Cozinha (KDS)
- [ ] Soft Delete
- [ ] Permissões avançadas via EmpresaUsuario

---

## Documentos Principais

- [[Orion-ERP]] — Fonte oficial de verdade (arquitetura, invariantes, contratos)
- [[Arquitetura Geral]] — Stack e estrutura de camadas
- [[Módulo Departamento]] — Módulo de referência para novos CRUDs
- [[CQRS e MediatR]] — Padrão de implementação de features
- [[Módulos/Multi-Tenancy]] — Como o isolamento funciona

---

## Como Adicionar Nova Feature

Seguir o padrão de [[Módulo Departamento]]:

1. **Domain** → entidade + `ITenantAware` se necessário
2. **Infrastructure** → `IEntityTypeConfiguration<T>` + DbSet + Global Query Filter
3. **Application** → Commands, Queries, Models, Validators em `Features/NomeDaFeature/`
4. **API** → Controller herdando `BaseCrudController<>`

> Sempre validar: DRY · KISS · YAGNI · Segurança multi-tenant
