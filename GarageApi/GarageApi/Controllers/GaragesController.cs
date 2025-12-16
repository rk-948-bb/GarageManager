using GarageApi.Models;
using GarageApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GarageApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GaragesController : ControllerBase
    {
        private readonly GarageRepository _repo;

        public GaragesController(GarageRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("gov")]
        public async Task<IActionResult> GetGov([FromQuery] int limit = 5, CancellationToken ct = default)
        {
            var items = await _repo.FetchFromGovApiAsync(limit, ct);
            return Ok(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetStored(CancellationToken ct = default)
        {
            var items = await _repo.GetAllAsync(ct);
            return Ok(items);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulk([FromBody] IEnumerable<Garage> garages, CancellationToken ct = default)
        {
            var added = await _repo.AddBulkIfNotExistAsync(garages, ct);
            return Ok(new { added = added.Count, totalAttempt = garages.Count() });
        }

        [HttpPost]
        public async Task<IActionResult> AddSingle([FromBody] Garage g, CancellationToken ct = default)
        {
            var added = await _repo.AddAsync(g, ct);
            if (added == null) return Conflict("Already exists");
            return CreatedAtAction(nameof(GetStored), new { id = added.Id }, added);
        }
    }
}
