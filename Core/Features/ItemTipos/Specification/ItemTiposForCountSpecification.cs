using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.ItemTipos.Specification;

public class ItemTiposForCountSpecification : BaseSpecification<ItemTipo>
{
    public ItemTiposForCountSpecification(Guid proyectoId, string search)
        : base(it => it.ProyectoId == proyectoId && !it.IsDeleted &&
                    (string.IsNullOrEmpty(search) || it.Nombre.Contains(search) || it.Codigo.Contains(search)))
    {
    }
}
