using CondoManager.Api.Infrastructure;
using CondoManager.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly CondoContext _context;

        public EventsController(CondoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
            => await _context.Events.ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(Guid id)
        {
            var evt = await _context.Events.FindAsync(id);
            return evt is null ? NotFound() : Ok(evt);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event evt)
        {
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, evt);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(Guid id, Event evt)
        {
            if (id != evt.Id) return BadRequest();
            _context.Entry(evt).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt is null) return NotFound();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
