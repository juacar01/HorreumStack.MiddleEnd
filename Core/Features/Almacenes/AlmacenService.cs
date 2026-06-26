using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using AutoMapper;
using HorreumStack.Infrastructure.Repositories;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenService : IAlmacenService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AlmacenService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<List<AlmacenDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<AlmacenDto> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<AlmacenDto> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    public async Task<List<AlmacenDto>> GetListByUserIdAsync(Guid userId)
    {
        var includes = new List<Expression<Func<AlmacenUser, object>>>
        {
            au => au.Almacen
        };

        var userAlmacenes = await _unitOfWork.Repository<AlmacenUser>().GetAsync(
            au => au.UserId == userId,
            null,
            includes,
            true
        );

        var almacenes = userAlmacenes.Select(au => au.Almacen).ToList();
        return _mapper.Map<List<AlmacenDto>>(almacenes);
    }

    public Task<AlmacenDto> CreateAsync(AlmacenDto model)
    {
        throw new NotImplementedException();
    }

    public Task<AlmacenDto> UpdateAsync(Guid id, AlmacenDto model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}