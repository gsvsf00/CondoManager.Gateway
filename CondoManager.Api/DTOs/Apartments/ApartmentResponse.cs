namespace CondoManager.Api.DTOs.Apartments
{
    public class ApartmentResponse
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public IEnumerable<int> ResidentUserIds { get; set; } = new List<int>();
    }
}
