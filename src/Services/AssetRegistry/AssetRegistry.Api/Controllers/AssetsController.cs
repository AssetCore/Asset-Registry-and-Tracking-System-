using AssetRegistry.Application.Assets;
using AssetRegistry.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AssetRegistry.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        public AssetsController(IAssetService assetService) => _assetService = assetService;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Asset assetBody, CancellationToken cancellationToken)
        {
            var newId = await _assetService.CreateAsync(assetBody, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { id = newId });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var assetEntity = await _assetService.GetByIdAsync(id, cancellationToken);
            return assetEntity is null ? NotFound() : Ok(assetEntity);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var assets = await _assetService.ListAsync(page, pageSize, cancellationToken);
            return Ok(assets);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Asset assetBody, CancellationToken cancellationToken)
        {
            assetBody.Id = id; 
            var ok = await _assetService.UpdateAsync(assetBody, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            var ok = await _assetService.DeactivateAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
        {
            var ok = await _assetService.RestoreAsync(id, cancellationToken);
            return ok ? NoContent() : NotFound();
        }
    }
}