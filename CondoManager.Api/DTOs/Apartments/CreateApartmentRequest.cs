using System;
using System.Collections.Generic;

namespace CondoManager.Api.DTOs.Apartments
{
    public class CreateApartmentRequest
    {
        public string Number { get; set; } = string.Empty;
        public IEnumerable<Guid> ResidentUserIds { get; set; } = new List<Guid>();
    }
}
