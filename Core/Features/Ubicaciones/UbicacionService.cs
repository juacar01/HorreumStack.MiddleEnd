using AutoMapper;
using HorreumStack.Infrastructure.Repositories;

namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

public class UbicacionService : IUbicacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UbicacionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<List<UbicacionResponse>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<UbicacionResponse>> GetAllByAlmacenIdAsync(Guid almacenId)
    {
        throw new NotImplementedException();
    }

    public Task<UbicacionResponse> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<UbicacionResponse> CreateAsync(UbicacionDto model)
    {
        throw new NotImplementedException();
    }

    public Task<UbicacionResponse> UpdateAsync(Guid id, UbicacionDto model)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}