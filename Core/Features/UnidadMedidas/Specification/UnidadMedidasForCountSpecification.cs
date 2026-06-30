using System;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas.Specification;

public class UnidadMedidasForCountSpecification : BaseSpecification<UnidadMedida>
{
    public UnidadMedidasForCountSpecification(Guid proyectoId, string search)
        : base(um => um.ProyectoId == proyectoId && !um.IsDeleted &&
                    (string.IsNullOrEmpty(search) || um.Nombre.Contains(search) || um.Codigo.Contains(search)))
    {
    }
}
