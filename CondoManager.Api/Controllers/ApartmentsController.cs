using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CondoManager.Api.DTOs.Apartments;
using CondoManager.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondoManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApartmentsController : ControllerBase
    {
        private readonly IApartmentService _service;

        public ApartmentsController(IApartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApartmentResponse>>> GetApartments()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApartmentResponse>> GetApartment(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Trustee")]
        public async Task<ActionResult<ApartmentResponse>> PostApartment([FromBody] CreateApartmentRequest request)
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetApartment), new { id = created.Id }, created);
        }
    }
}
