using CondoManager.Api.DTOs.Apartments;

namespace CondoManager.Api.Interfaces
{
    public interface IApartmentService
    {
        Task<ApartmentResponse> CreateAsync(CreateApartmentRequest request);
        Task<IEnumerable<ApartmentResponse>> GetAllAsync();
        Task<ApartmentResponse?> GetByIdAsync(Guid id);
    }
}
