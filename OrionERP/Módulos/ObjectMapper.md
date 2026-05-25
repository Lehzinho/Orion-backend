---
tags: [orionerp, mapeamento, performance, expression-trees, dto]
relacionado: 
    - "[[Arquitetura Geral]]" 
    - "[[CQRS e MediatR]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# ObjectMapper

Utilitário de mapeamento entre objetos baseado em expression trees com cache. Substitui AutoMapper — sem reflection em runtime após o primeiro uso, sem dependências externas e com comportamento determinístico.

## Como funciona

Funciona como método de extensão em `object`, disponível em toda a application layer:

```csharp
// Mapear objeto único
var response = entity.MapTo<EmpresaResponse>();

// Mapear lista
var responses = entities.MapListTo<EmpresaResponse>();
```

**Mecanismo interno:**
1. Na primeira chamada para um par `(SourceType, DestinationType)`, compila uma `Expression<Func<object, TDestination>>` que copia propriedades por nome
2. A expressão compilada é armazenada em `ConcurrentDictionary<string, Delegate>` com chave `SourceType_DestinationType`
3. Chamadas subsequentes usam o delegate em cache — zero overhead de reflection

**Regras de mapeamento:**
- Copia apenas propriedades com mesmo nome
- Suporta apenas tipos simples (`string`, `bool`, `int`, `long`, `decimal`, `DateTime`, `Guid`, etc.)
- Ignora propriedades marcadas com `[IgnoreMap]`
- Tenta conversão implícita entre tipos compatíveis (ex: `bool` ↔ `bool?`)
- Propriedades sem correspondência ou de tipos incompatíveis são silenciosamente ignoradas

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Application/Common/Mapping/ObjectMapper.cs` | Implementação com cache de expression trees |
| `OrionERP.Application/Common/Mapping/IgnoreMappAttribute.cs` | Atributo `[IgnoreMap]` para excluir propriedades do mapeamento |
| `OrionERP.Domain/Common/IgnoreUpdateAttribute.cs` | Atributo `[IgnoreUpdate]` usado para excluir campos do `UpdateExtensions` |

## Integrações

- Usado nos Handlers para converter `Entity → Response` e `Request → Entity`
- Complementado por `UpdateExtensions.cs` para aplicar updates parciais (respeita `[IgnoreUpdate]`)

## Configuração

Nenhuma configuração necessária — é uma classe estática com métodos de extensão. Disponível via `using OrionERP.Application.Common.Mapping`.

## Observações importantes

- **Nunca introduzir AutoMapper** — decisão arquitetural registrada em `decisions.md`
- Não mapeia coleções de entidades aninhadas — apenas propriedades simples
- Se precisar mapear propriedades calculadas ou com transformação, faça manualmente no Handler após o `MapTo<>`
- Thread-safe por design (`ConcurrentDictionary`)
