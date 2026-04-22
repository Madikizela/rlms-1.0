using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsDevelopmentProvidersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SkillsDevelopmentProvidersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SkillsDevelopmentProviders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillsDevelopmentProvider>>> GetSkillsDevelopmentProviders()
        {
            return await _context.SkillsDevelopmentProviders
                .Include(s => s.Client)
                .Include(s => s.Users)
                .Include(s => s.Departments)
                .ToListAsync();
        }

        // GET: api/SkillsDevelopmentProviders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkillsDevelopmentProvider>> GetSkillsDevelopmentProvider(int id)
        {
            var sdp = await _context.SkillsDevelopmentProviders
                .Include(s => s.Client)
                .Include(s => s.Users)
                .Include(s => s.Departments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sdp == null)
            {
                return NotFound();
            }

            return sdp;
        }

        // GET: api/SkillsDevelopmentProviders/ByClient/{clientId}
        [HttpGet("ByClient/{clientId}")]
        public async Task<ActionResult<IEnumerable<SkillsDevelopmentProvider>>> GetSDPsByClient(int clientId)
        {
            return await _context.SkillsDevelopmentProviders
                .Where(s => s.ClientId == clientId)
                .Include(s => s.Client)
                .Include(s => s.Users)
                .Include(s => s.Departments)
                .ToListAsync();
        }

        // GET: api/SkillsDevelopmentProviders/{id}/Users
        [HttpGet("{id}/Users")]
        public async Task<ActionResult<IEnumerable<User>>> GetSDPUsers(int id)
        {
            var sdp = await _context.SkillsDevelopmentProviders
                .Include(s => s.Users)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sdp == null)
            {
                return NotFound();
            }

            return Ok(sdp.Users);
        }

        // GET: api/SkillsDevelopmentProviders/{id}/Departments
        [HttpGet("{id}/Departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetSDPDepartments(int id)
        {
            var sdp = await _context.SkillsDevelopmentProviders
                .Include(s => s.Departments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sdp == null)
            {
                return NotFound();
            }

            return Ok(sdp.Departments);
        }

        // PUT: api/SkillsDevelopmentProviders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkillsDevelopmentProvider(int id, SkillsDevelopmentProvider sdp)
        {
            if (id != sdp.Id)
            {
                return BadRequest();
            }

            sdp.UpdatedAt = DateTime.UtcNow;
            _context.Entry(sdp).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkillsDevelopmentProviderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SkillsDevelopmentProviders
        [HttpPost]
        public async Task<ActionResult<SkillsDevelopmentProvider>> PostSkillsDevelopmentProvider(SkillsDevelopmentProvider sdp)
        {
            sdp.CreatedAt = DateTime.UtcNow;
            sdp.UpdatedAt = DateTime.UtcNow;
            
            _context.SkillsDevelopmentProviders.Add(sdp);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSkillsDevelopmentProvider", new { id = sdp.Id }, sdp);
        }

        // DELETE: api/SkillsDevelopmentProviders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkillsDevelopmentProvider(int id)
        {
            var sdp = await _context.SkillsDevelopmentProviders.FindAsync(id);
            if (sdp == null)
            {
                return NotFound();
            }

            _context.SkillsDevelopmentProviders.Remove(sdp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SkillsDevelopmentProviderExists(int id)
        {
            return _context.SkillsDevelopmentProviders.Any(e => e.Id == id);
        }
    }
}