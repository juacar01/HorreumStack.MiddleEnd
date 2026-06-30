using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos.Specification;

public class UbicacionesTiposSpecification : BaseSpecification<UbicacionTipo>
{
    public UbicacionesTiposSpecification(Guid proyectoId, int pageIndex, int pageSize, string search)
        : base(ut => ut.ProyectoId == proyectoId && !ut.IsDeleted &&
                    (string.IsNullOrEmpty(search) || ut.Nombre.Contains(search) || ut.Codigo.Contains(search)))
    {
        AddInclude(ut => ut.Proyecto!);
        AddOrderByDescending(ut => ut.CreatedAt!);
        ApplyPaging(pageSize * (pageIndex - 1), pageSize);
    }
}
