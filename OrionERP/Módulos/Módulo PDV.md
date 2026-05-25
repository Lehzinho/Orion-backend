---
tags:
  - orionerp
  - pdv
  - ponto-de-venda
  - empresa-scoped
  - restaurante
  - pendente
relacionado:
  - "[[Módulo Empresa]]"
  - "[[Módulo Almoxarifado]]"
  - "[[Módulo Departamento]]"
  - "[[Módulo Centro de Custo]]"
  - "[[Módulos/Multi-Tenancy]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo PDV

Ponto de Venda vinculado a uma Empresa. Representa uma estação/terminal de vendas com configurações de produção, taxa de serviço e almoxarifado associado. É a entidade central para o MVP de restaurante.

## Como funciona

**Entidade `Pdv`:**
- EmpresaScoped — filtro global por `EmpresaId`
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `DepartamentoId`, `CentroCustoId`, `AlmoxarifadoId`, `Codigo`, `Nome`, `PdvDefault`, `Ativo`, `TipoPdv`, `ProducaoItem`, `NumPreparo`, `CobrarTaxaServico`, `ExternalId`
- Relacionamentos: pertence a `Empresa`, `Almoxarifado`, `CentroCusto`, `Departamento`

**Estado atual:**
- ✅ Entidade de domínio implementada
- ✅ Configuração EF (`PdvConfiguration.cs`) implementada
- ✅ DbSet registrado no `OrionDbContext`
- ⚠️ Sem CRUD — não há Features (Commands/Queries), Controller, nem endpoints expostos
- ⚠️ Não aparece no Global Query Filter do `OrionDbContext` ainda — **verificar**

> PDV está na Fase 1 pendente de implementação completa do CRUD.

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/Pdv.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/PdvConfiguration.cs` | Configuração EF |
| `OrionERP.Infrastructure/Persistence/OrionDbContext.cs` | DbSet `Pdvs` registrado; query filter por `EmpresaId` |

## Integrações

- [[Módulo Empresa]] — FK obrigatória para `EmpresaId`
- [[Módulo Almoxarifado]] — FK obrigatória para `AlmoxarifadoId`
- [[Módulo Departamento]] — FK obrigatória para `DepartamentoId`
- [[Módulo Centro de Custo]] — FK obrigatória para `CentroCustoId`

## Configuração

Ainda não implementado como endpoint. Para implementar, seguir o padrão do [[Módulo Departamento]].

## Observações importantes

- Invariante: Pdv deve pertencer a uma Empresa
- `TipoPdv` define o tipo do terminal (valores aceitos a documentar quando CRUD for implementado)
- `NumPreparo` e `ProducaoItem` são campos ligados ao fluxo de produção do restaurante
- `PdvDefault` indica o PDV padrão da empresa
- Próximo passo: implementar Features (Commands + Queries) e Controller seguindo padrão de Almoxarifados
