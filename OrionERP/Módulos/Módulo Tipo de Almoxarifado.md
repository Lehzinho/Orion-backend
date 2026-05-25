---
tags: [orionerp, tipo-almoxarifado, crud, global, catalogo]
relacionado: 
    - "[[Módulo Almoxarifado]]" 
    - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Tipo de Almoxarifado

Catálogo global de tipos/categorias de almoxarifado. Define a classificação de um almoxarifado (ex: Geral, Farmácia, Matéria-Prima). É uma entidade global — não é filtrada por tenant nem por empresa.

## Como funciona

**Entidade `TipoAlmoxarifado`:**
- Escopo: **Global** — sem `ITenantAware`, sem filtros de tenant/empresa
- Campos: `Id`, `Codigo`, `Nome`, `Ativo`
- Relacionamentos: tem muitos `Almoxarifados`

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/tiposalmoxarifados` |
| Buscar | `GET /api/tiposalmoxarifados/{id}` |
| Listar | `GET /api/tiposalmoxarifados?Nome.contains=...` |
| Atualizar | `PUT /api/tiposalmoxarifados/{id}` |
| Deletar | `DELETE /api/tiposalmoxarifados/{id}` |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/TipoAlmoxarifado.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/TipoAlmoxarifadoConfiguration.cs` | Configuração EF |
| `OrionERP.API/Controllers/TiposAlmoxarifadosController.cs` | Controller HTTP |
| `OrionERP.Application/Features/TiposAlmoxarifados/Commands/` | Create, Update, Delete handlers + validators |
| `OrionERP.Application/Features/TiposAlmoxarifados/Queries/` | GetById e GetAll |
| `OrionERP.Application/Features/TiposAlmoxarifados/Models/` | DTOs e modelos |

## Integrações

- [[Módulo Almoxarifado]] — `Almoxarifado.TipoAlmoxarifadoId` é FK obrigatória para `TipoAlmoxarifado`

## Configuração

Por ser global, não tem filtros de tenant — qualquer requisição autenticada pode ler/criar tipos.

## Observações importantes

- Entidade global — não exposta a riscos de vazamento entre tenants (não tem dados sensíveis)
- Invariante: Almoxarifado deve sempre referenciar um `TipoAlmoxarifado` válido
- Geralmente populado via seed ou cadastro administrativo único
