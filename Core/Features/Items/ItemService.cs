using System.Linq.Expressions;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.MiddleEnd.Core.Constants;
using HorreumStack.MiddleEnd.Core.Features.Items.Specification;
using MiniExcelLibs;
using System.IO;

namespace HorreumStack.MiddleEnd.Core.Features.Items;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ItemResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Item>().GetAsync(
            i => !i.IsDeleted
        );
        return _mapper.Map<List<ItemResponse>>(list);
    }

    public async Task<List<ItemResponse>> GetAllByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<Item>().GetAsync(
            i => i.ProyectoId == proyectoId && !i.IsDeleted
        );
        return _mapper.Map<List<ItemResponse>>(list);
    }

    public async Task<ItemPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search)
    {
        var spec = new ItemsWithTypesSpecification(proyectoId, pageIndex, pageSize, search ?? "");
        var countSpec = new ItemsWithFiltersForCountSpecification(proyectoId, search ?? "");

        var totalItems = await _unitOfWork.Repository<Item>().CountAsync(countSpec);
        var itemsList = await _unitOfWork.Repository<Item>().GetAllWithSpec(spec);

        var data = _mapper.Map<IReadOnlyList<ItemResponse>>(itemsList);

        return new ItemPaginationResponse
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalItems,
            Data = data
        };
    }

    public async Task<ItemResponse> GetByIdAsync(Guid id)
    {
        var includes = new List<Expression<Func<Item, object>>>
        {
            i => i.Proyecto!,
            i => i.ItemTipo!
        };

        var item = await _unitOfWork.Repository<Item>().GetEntityAsync(
            i => i.Id == id && !i.IsDeleted,
            includes,
            true
        );

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> CreateAsync(ItemDto model)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var item = _mapper.Map<Item>(model);
        item.Id = Guid.NewGuid();
        item.Codigo = AppConstants.Items.PrefixCodigo + codigoGenerado;
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = model.CreatedBy ?? "System";
        item.LastModifiedAt = DateTime.UtcNow;
        item.LastModifiedBy = model.CreatedBy ?? "System";

        await _unitOfWork.Repository<Item>().AddAsync(item);
        await _unitOfWork.Complete();

        return _mapper.Map<ItemResponse>(item);
    }

    public async Task<ItemResponse> UpdateAsync(Guid id, ItemDto model)
    {
        var existing = await _unitOfWork.Repository<Item>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Item con ID {id} no encontrado");

        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.Items.PrefixCodigo + model.Codigo.Replace(AppConstants.Items.PrefixCodigo, "");
        existing.Descripcion = model.Descripcion;
        existing.Imagen = model.Imagen;
        existing.ItemTipoId = model.ItemTipoId;
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = model.LastModifiedBy ?? "System";

        await _unitOfWork.Repository<Item>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<ItemResponse>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<Item>().GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Item>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }

    public async Task<int> ImportItemsAsync(Guid proyectoId, Stream fileStream, string fileExtension, string createdBy)
    {
        var excelType = fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase) ? ExcelType.CSV : ExcelType.XLSX;
        var rows = fileStream.Query(excelType: excelType).Cast<IDictionary<string, object>>().ToList();
        
        if (!rows.Any()) return 0;

        // Fetch existing item types for this project
        var existingTypes = (await _unitOfWork.Repository<ItemTipo>().GetAsync(t => t.ProyectoId == proyectoId && !t.IsDeleted)).ToList();

        var itemsToAdd = new List<Item>();
        int importedCount = 0;

        foreach (var row in rows)
        {
            string nombre = GetValueFromRow(row, "Nombre");
            if (string.IsNullOrEmpty(nombre)) continue; // Nombre is required

            string codigo = GetValueFromRow(row, "Codigo");
            string descripcion = GetValueFromRow(row, "Descripcion");
            string imagen = GetValueFromRow(row, "Imagen");
            string tipoNombre = GetValueFromRow(row, "Tipo");

            Guid? itemTipoId = null;
            if (!string.IsNullOrEmpty(tipoNombre))
            {
                var existingType = existingTypes.FirstOrDefault(t => t.Nombre.Equals(tipoNombre, StringComparison.OrdinalIgnoreCase));
                if (existingType != null)
                {
                    itemTipoId = existingType.Id;
                }
                else
                {
                    // Create new ItemTipo dynamically
                    var newType = new ItemTipo
                    {
                        Id = Guid.NewGuid(),
                        Nombre = tipoNombre,
                        Codigo = AppConstants.ItemTipos.PrefixCodigo + Guid.NewGuid().ToString().Substring(0, 8),
                        ProyectoId = proyectoId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        LastModifiedAt = DateTime.UtcNow,
                        LastModifiedBy = createdBy
                    };
                    await _unitOfWork.Repository<ItemTipo>().AddAsync(newType);
                    existingTypes.Add(newType); // Add to cache list
                    itemTipoId = newType.Id;
                }
            }

            var codigoGenerado = string.IsNullOrEmpty(codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : codigo;
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Codigo = AppConstants.Items.PrefixCodigo + codigoGenerado.Replace(AppConstants.Items.PrefixCodigo, ""),
                Descripcion = descripcion ?? "",
                Imagen = imagen ?? "",
                ProyectoId = proyectoId,
                ItemTipoId = itemTipoId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = createdBy
            };

            itemsToAdd.Add(item);
            importedCount++;
        }

        if (itemsToAdd.Any())
        {
            foreach (var item in itemsToAdd)
            {
                await _unitOfWork.Repository<Item>().AddAsync(item);
            }
            await _unitOfWork.Complete();
        }

        return importedCount;
    }

    private string GetValueFromRow(IDictionary<string, object> row, string keyName)
    {
        var key = row.Keys.FirstOrDefault(k => k.Equals(keyName, StringComparison.OrdinalIgnoreCase));
        if (key != null && row[key] != null)
        {
            return row[key].ToString() ?? "";
        }
        return "";
    }
}
