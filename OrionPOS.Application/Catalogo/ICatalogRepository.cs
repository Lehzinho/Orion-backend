namespace OrionPOS.Application.Catalogo;

public interface ICatalogRepository
{
    Task<CardapioDto?> GetCardapioPadraoAsync(Guid tenantId, CancellationToken ct);
}
