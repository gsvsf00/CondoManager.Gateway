namespace CondoManager.Api.DTOs.Apartments
{
    public class ApartmentResponse
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public IEnumerable<Guid> ResidentUserIds { get; set; } = new List<Guid>();
    }
}
