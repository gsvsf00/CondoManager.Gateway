using CondoManager.Api.Interfaces;
using CondoManager.Api.Infrastructure;
using CondoManager.Entity.Models;
using CondoManager.Entity.Events;
using CondoManager.Repository.Interfaces;

namespace CondoManager.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RabbitMqPublisher _eventPublisher;

        public UserService(IUnitOfWork unitOfWork, RabbitMqPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserWithApartmentsAsync(Guid id)
        {
            return await _unitOfWork.Users.GetWithApartmentsAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetUsersByApartmentIdAsync(Guid apartmentId)
        {
            return await _unitOfWork.Users.GetByApartmentIdAsync(apartmentId);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!await UserExistsAsync(user.Id))
                throw new InvalidOperationException($"User with ID {user.Id} does not exist");

            var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException($"User with ID {user.Id} not found");

            // Update properties
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.Roles = user.Roles;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            // Publish user updated event
            var userUpdatedEvent = new UserUpdatedEvent
            {
                UserId = existingUser.Id,
                Email = existingUser.Email,
                Name = existingUser.Name.Split(' ').FirstOrDefault() ?? ""
            };
            await _eventPublisher.PublishAsync(userUpdatedEvent, "user.updated");

            return existingUser;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            // Publish user deleted event before deletion
            var userDeletedEvent = new UserDeletedEvent
            {
                UserId = user.Id,
                Email = user.Email
            };
            await _eventPublisher.PublishAsync(userDeletedEvent, "user.deleted");

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _unitOfWork.Users.EmailExistsAsync(email);
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _unitOfWork.Users.PhoneExistsAsync(phone);
        }
    }
}