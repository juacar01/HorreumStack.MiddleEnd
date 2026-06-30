using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public interface IInventarioService
{
    Task<List<InventarioResponse>> GetInventarioByUbicacionAsync(Guid ubicacionId, CancellationToken cancellationToken);
    Task<List<InventarioMovimientoResponse>> GetMovimientosByUbicacionAsync(Guid ubicacionId, CancellationToken cancellationToken);
    Task CargarStockAsync(Guid ubicacionId, CargarInventarioDto dto, string userEmail, CancellationToken cancellationToken);
    Task DescargarStockAsync(Guid ubicacionId, CargarInventarioDto dto, string userEmail, CancellationToken cancellationToken);
    Task MoverStockAsync(Guid ubicacionOrigenId, MoverInventarioDto dto, string userEmail, CancellationToken cancellationToken);
}
