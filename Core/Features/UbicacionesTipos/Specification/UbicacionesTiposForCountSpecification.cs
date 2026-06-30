using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos.Specification;

public class UbicacionesTiposForCountSpecification : BaseSpecification<UbicacionTipo>
{
    public UbicacionesTiposForCountSpecification(Guid proyectoId, string search)
        : base(ut => ut.ProyectoId == proyectoId && !ut.IsDeleted &&
                    (string.IsNullOrEmpty(search) || ut.Nombre.Contains(search) || ut.Codigo.Contains(search)))
    {
    }
}
