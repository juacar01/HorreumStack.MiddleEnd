using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.ItemTipos.Specification;

public class ItemTiposSpecification : BaseSpecification<ItemTipo>
{
    public ItemTiposSpecification(Guid proyectoId, int pageIndex, int pageSize, string search)
        : base(it => it.ProyectoId == proyectoId && !it.IsDeleted &&
                    (string.IsNullOrEmpty(search) || it.Nombre.Contains(search) || it.Codigo.Contains(search)))
    {
        AddInclude(it => it.Proyecto!);
        AddOrderByDescending(it => it.CreatedAt!);
        ApplyPaging(pageSize * (pageIndex - 1), pageSize);
    }
}
