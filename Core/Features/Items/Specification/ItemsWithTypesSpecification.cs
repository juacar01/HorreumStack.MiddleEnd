using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.Items.Specification;

public class ItemsWithTypesSpecification : BaseSpecification<Item>
{
    public ItemsWithTypesSpecification(Guid proyectoId, int pageIndex, int pageSize, string search)
        : base(i => i.ProyectoId == proyectoId && !i.IsDeleted &&
                   (string.IsNullOrEmpty(search) || i.Nombre.Contains(search) || i.Codigo.Contains(search)))
    {
        AddInclude(i => i.Proyecto!);
        AddInclude(i => i.ItemTipo!);
        AddInclude(i => i.UnidadMedida!);
        AddOrderByDescending(i => i.CreatedAt!);
        ApplyPaging(pageSize * (pageIndex - 1), pageSize);
    }
}
