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
            i => i.ItemTipo!,
            i => i.UnidadMedida!
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
        existing.UnidadMedidaId = model.UnidadMedidaId;
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
        var isCsv = fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
        
        // Cache collections for dynamic types and units
        var existingTypes = (await _unitOfWork.Repository<ItemTipo>().GetAsync(t => t.ProyectoId == proyectoId && !t.IsDeleted)).ToList();
        var existingUnits = (await _unitOfWork.Repository<UnidadMedida>().GetAsync(u => u.ProyectoId == proyectoId && !u.IsDeleted)).ToList();
        
        var itemsToAdd = new List<Item>();
        int importedCount = 0;

        if (isCsv)
        {
            using var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8);
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine)) return 0;

            var headers = ParseCsvLine(headerLine, ';').Select(h => h.Trim()).ToList();
            int indexNombre = headers.FindIndex(h => h.Equals("Nombre", StringComparison.OrdinalIgnoreCase));
            int indexCodigo = headers.FindIndex(h => h.Equals("Codigo", StringComparison.OrdinalIgnoreCase));
            int indexDescripcion = headers.FindIndex(h => h.Equals("Descripcion", StringComparison.OrdinalIgnoreCase));
            int indexImagen = headers.FindIndex(h => h.Equals("Imagen", StringComparison.OrdinalIgnoreCase));
            int indexTipo = headers.FindIndex(h => h.Equals("Tipo", StringComparison.OrdinalIgnoreCase));
            int indexUnidad = headers.FindIndex(h => h.Equals("UnidadMedida", StringComparison.OrdinalIgnoreCase));

            if (indexNombre < 0)
            {
                throw new InvalidDataException("La columna 'Nombre' es requerida en el encabezado del archivo CSV.");
            }

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var fields = ParseCsvLine(line, ';');

                string nombre = indexNombre < fields.Count ? StripWrappingQuotes(fields[indexNombre]) : "";
                if (string.IsNullOrEmpty(nombre)) continue; // Nombre is required

                string codigo = indexCodigo >= 0 && indexCodigo < fields.Count ? StripWrappingQuotes(fields[indexCodigo]) : "";
                string descripcion = indexDescripcion >= 0 && indexDescripcion < fields.Count ? StripWrappingQuotes(fields[indexDescripcion]) : "";
                string imagen = indexImagen >= 0 && indexImagen < fields.Count ? StripWrappingQuotes(fields[indexImagen]) : "";
                string tipoNombre = indexTipo >= 0 && indexTipo < fields.Count ? StripWrappingQuotes(fields[indexTipo]) : "";
                string unidadMedidaNombre = indexUnidad >= 0 && indexUnidad < fields.Count ? StripWrappingQuotes(fields[indexUnidad]) : "";

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
                        existingTypes.Add(newType);
                        itemTipoId = newType.Id;
                    }
                }

                Guid? unidadMedidaId = null;
                if (!string.IsNullOrEmpty(unidadMedidaNombre))
                {
                    var existingUnit = existingUnits.FirstOrDefault(u => u.Nombre.Equals(unidadMedidaNombre, StringComparison.OrdinalIgnoreCase));
                    if (existingUnit != null)
                    {
                        unidadMedidaId = existingUnit.Id;
                    }
                    else
                    {
                        var newUnit = new UnidadMedida
                        {
                            Id = Guid.NewGuid(),
                            Nombre = unidadMedidaNombre,
                            Codigo = AppConstants.UnidadMedidas.PrefixCodigo + Guid.NewGuid().ToString().Substring(0, 8),
                            ProyectoId = proyectoId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = createdBy,
                            LastModifiedAt = DateTime.UtcNow,
                            LastModifiedBy = createdBy
                        };
                        await _unitOfWork.Repository<UnidadMedida>().AddAsync(newUnit);
                        existingUnits.Add(newUnit);
                        unidadMedidaId = newUnit.Id;
                    }
                }

                var codigoGenerado = string.IsNullOrEmpty(codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : codigo;
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Nombre = nombre,
                    Codigo = AppConstants.Items.PrefixCodigo + codigoGenerado.Replace(AppConstants.Items.PrefixCodigo, ""),
                    Descripcion = descripcion,
                    Imagen = imagen,
                    ProyectoId = proyectoId,
                    ItemTipoId = itemTipoId,
                    UnidadMedidaId = unidadMedidaId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = createdBy
                };

                itemsToAdd.Add(item);
                importedCount++;
            }
        }
        else
        {
            // MiniExcel path for Excel binary files
            var excelType = ExcelType.XLSX;
            var rows = fileStream.Query(excelType: excelType).Cast<IDictionary<string, object>>().ToList();
            if (!rows.Any()) return 0;

            foreach (var row in rows)
            {
                string nombre = GetValueFromRow(row, "Nombre");
                if (string.IsNullOrEmpty(nombre)) continue;

                string codigo = GetValueFromRow(row, "Codigo");
                string descripcion = GetValueFromRow(row, "Descripcion");
                string imagen = GetValueFromRow(row, "Imagen");
                string tipoNombre = GetValueFromRow(row, "Tipo");
                string unidadMedidaNombre = GetValueFromRow(row, "UnidadMedida");

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
                        existingTypes.Add(newType);
                        itemTipoId = newType.Id;
                    }
                }

                Guid? unidadMedidaId = null;
                if (!string.IsNullOrEmpty(unidadMedidaNombre))
                {
                    var existingUnit = existingUnits.FirstOrDefault(u => u.Nombre.Equals(unidadMedidaNombre, StringComparison.OrdinalIgnoreCase));
                    if (existingUnit != null)
                    {
                        unidadMedidaId = existingUnit.Id;
                    }
                    else
                    {
                        var newUnit = new UnidadMedida
                        {
                            Id = Guid.NewGuid(),
                            Nombre = unidadMedidaNombre,
                            Codigo = AppConstants.UnidadMedidas.PrefixCodigo + Guid.NewGuid().ToString().Substring(0, 8),
                            ProyectoId = proyectoId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = createdBy,
                            LastModifiedAt = DateTime.UtcNow,
                            LastModifiedBy = createdBy
                        };
                        await _unitOfWork.Repository<UnidadMedida>().AddAsync(newUnit);
                        existingUnits.Add(newUnit);
                        unidadMedidaId = newUnit.Id;
                    }
                }

                var codigoGenerado = string.IsNullOrEmpty(codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : codigo;
                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    Nombre = nombre,
                    Codigo = AppConstants.Items.PrefixCodigo + codigoGenerado.Replace(AppConstants.Items.PrefixCodigo, ""),
                    Descripcion = descripcion,
                    Imagen = imagen,
                    ProyectoId = proyectoId,
                    ItemTipoId = itemTipoId,
                    UnidadMedidaId = unidadMedidaId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = createdBy
                };

                itemsToAdd.Add(item);
                importedCount++;
            }
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
            return StripWrappingQuotes(row[key].ToString());
        }
        return "";
    }

    private string StripWrappingQuotes(string? value)
    {
        if (value == null) return "";
        value = value.Trim();
        if (value.Length >= 2)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }
            else if (value.StartsWith("'") && value.EndsWith("'"))
            {
                value = value.Substring(1, value.Length - 2);
            }
        }
        return value;
    }

    private List<string> ParseCsvLine(string line, char separator)
    {
        var result = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes)
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        // Toggle off only if it's followed by a separator or end of line
                        if (i + 1 == line.Length || line[i + 1] == separator)
                        {
                            inQuotes = false;
                        }
                        else
                        {
                            currentField.Append('"');
                        }
                    }
                }
                else
                {
                    // Toggle on only if it is the start of the field
                    if (currentField.Length == 0)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        currentField.Append('"');
                    }
                }
            }
            else if (c == separator && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        result.Add(currentField.ToString());
        return result;
    }
}
