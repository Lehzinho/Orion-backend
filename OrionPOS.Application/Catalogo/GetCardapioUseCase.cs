namespace OrionPOS.Application.Catalogo;

public sealed class GetCardapioUseCase(ICatalogRepository repo)
{
    // tenantId virá do JWT futuramente; por ora usa um fixo do config
    public Task<CardapioDto?> ExecuteAsync(Guid tenantId, CancellationToken ct)
        => repo.GetCardapioPadraoAsync(tenantId, ct);
}
