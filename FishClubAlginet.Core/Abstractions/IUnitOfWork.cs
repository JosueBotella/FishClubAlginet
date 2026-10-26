using ErrorOr;

namespace FishClubAlginet.Application.Abstractions;

/// <summary>
/// Cierra un caso de uso: persiste en una única transacción todo lo que los
/// repositorios han "stageado" en el DbContext durante la petición.
/// Los repositorios se inyectan directamente en los handlers; el UoW solo confirma.
/// Application no conoce DbUpdateException: los fallos de EF Core llegan mapeados a ErrorOr.
/// </summary>
public interface IUnitOfWork
{
    Task<ErrorOr<int>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
