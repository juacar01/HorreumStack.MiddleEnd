using System;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas.Specification;

public class UnidadMedidasSpecification : BaseSpecification<UnidadMedida>
{
    public UnidadMedidasSpecification(Guid proyectoId, int pageIndex, int pageSize, string search)
        : base(um => um.ProyectoId == proyectoId && !um.IsDeleted &&
                    (string.IsNullOrEmpty(search) || um.Nombre.Contains(search) || um.Codigo.Contains(search)))
    {
        AddInclude(um => um.Proyecto!);
        AddOrderByDescending(um => um.CreatedAt!);
        ApplyPaging(pageSize * (pageIndex - 1), pageSize);
    }
}
