using CondoManager.Entity.Models;

namespace CondoManager.Api.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserWithApartmentsAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByApartmentIdAsync(Guid apartmentId);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task<bool> UserExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
    }
}