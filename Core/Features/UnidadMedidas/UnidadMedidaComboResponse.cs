using System;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

public class UnidadMedidaComboResponse
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
