using CondoManager.Api.DTOs.Apartments;

namespace CondoManager.Api.Services.Interfaces
{
    public interface IApartmentService
    {
        Task<ApartmentResponse> CreateAsync(CreateApartmentRequest request);
        Task<IEnumerable<ApartmentResponse>> GetAllAsync();
        Task<ApartmentResponse?> GetByIdAsync(int id);
    }
}
