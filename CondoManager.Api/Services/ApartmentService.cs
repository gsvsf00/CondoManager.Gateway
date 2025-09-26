using CondoManager.Api.DTOs.Apartments;
using CondoManager.Api.Infrastructure;
using CondoManager.Api.Interfaces;
using CondoManager.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Services
{
    public class ApartmentService : IApartmentService
    {
        private readonly CondoContext _db;

        public ApartmentService(CondoContext db)
        {
            _db = db;
        }

        public async Task<ApartmentResponse> CreateAsync(CreateApartmentRequest request)
        {
            var entity = new Apartment { Id = Guid.NewGuid(), Number = request.Number };

            // Validate and attach residents by user IDs if provided
            if (request.ResidentUserIds != null)
            {
                var userIds = request.ResidentUserIds.Distinct().ToList();
                if (userIds.Count > 0)
                {
                    var existingIds = await _db.Users.Where(u => userIds.Contains(u.Id))
                                                     .Select(u => u.Id)
                                                     .ToListAsync();
                    var missing = userIds.Except(existingIds).ToList();
                    if (missing.Count > 0)
                        throw new InvalidOperationException($"Users not found: {string.Join(", ", missing)}");

                    entity.Residents = userIds.Select(uid => new ApartmentUser
                    {
                        ApartmentId = entity.Id,
                        UserId = uid
                    }).ToList();
                }
            }

            _db.Apartments.Add(entity);
            await _db.SaveChangesAsync();

            return new ApartmentResponse
            {
                Id = entity.Id,
                Number = entity.Number,
                ResidentUserIds = entity.Residents.Select(r => r.UserId)
            };
        }

        public async Task<IEnumerable<ApartmentResponse>> GetAllAsync()
        {
            var list = await _db.Apartments.Include(a => a.Residents).ToListAsync();
            return list.Select(a => new ApartmentResponse
            {
                Id = a.Id,
                Number = a.Number,
                ResidentUserIds = a.Residents.Select(r => r.UserId)
            });
        }

        public async Task<ApartmentResponse?> GetByIdAsync(Guid id)
        {
            var a = await _db.Apartments.Include(x => x.Residents).FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return null;
            return new ApartmentResponse
            {
                Id = a.Id,
                Number = a.Number,
                ResidentUserIds = a.Residents.Select(r => r.UserId)
            };
        }
    }
}
