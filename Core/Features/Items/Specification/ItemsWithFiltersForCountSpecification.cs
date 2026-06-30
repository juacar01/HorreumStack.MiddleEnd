using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.Items.Specification;

public class ItemsWithFiltersForCountSpecification : BaseSpecification<Item>
{
    public ItemsWithFiltersForCountSpecification(Guid proyectoId, string search)
        : base(i => i.ProyectoId == proyectoId && !i.IsDeleted &&
                   (string.IsNullOrEmpty(search) || i.Nombre.Contains(search) || i.Codigo.Contains(search)))
    {
    }
}
