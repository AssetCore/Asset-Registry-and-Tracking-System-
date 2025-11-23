using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MaintenanceScheduler.Application.DTOs;
using MaintenanceScheduler.Application.Services;

namespace MaintenanceScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceHistoryController : ControllerBase
    {
        private readonly IMaintenanceHistoryService _historyService;

        public MaintenanceHistoryController(IMaintenanceHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _historyService.GetByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("asset/{assetId}")]
        public async Task<IActionResult> GetByAssetId(Guid assetId)
        {
            var response = await _historyService.GetByAssetIdAsync(assetId);
            return Ok(response);
        }

        [HttpGet("asset/{assetId}/daterange")]
        public async Task<IActionResult> GetByDateRange(
            Guid assetId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest("Start date must be before end date");

            var response = await _historyService.GetByDateRangeAsync(assetId, startDate, endDate);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceHistoryDto dto)
        {
            var userId = User?.Identity?.Name ?? "System";
            var response = await _historyService.CreateAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _historyService.DeleteAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}