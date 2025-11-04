using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MaintenanceScheduler.Application.DTOs;
using MaintenanceScheduler.Application.Services;
using FluentValidation;

namespace MaintenanceScheduler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceScheduleController : ControllerBase
    {
        private readonly IMaintenanceScheduleService _scheduleService;
        private readonly IValidator<CreateMaintenanceScheduleDto> _createValidator;

        public MaintenanceScheduleController(
            IMaintenanceScheduleService scheduleService,
            IValidator<CreateMaintenanceScheduleDto> createValidator)
        {
            _scheduleService = scheduleService;
            _createValidator = createValidator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _scheduleService.GetByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _scheduleService.GetAllAsync();
            return Ok(response);
        }

        [HttpGet("asset/{assetId}")]
        public async Task<IActionResult> GetByAssetId(Guid assetId)
        {
            var response = await _scheduleService.GetByAssetIdAsync(assetId);
            return Ok(response);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming([FromQuery] int days = 30)
        {
            if (days <= 0)
                return BadRequest("Days parameter must be greater than 0");

            var response = await _scheduleService.GetUpcomingAsync(days);
            return Ok(response);
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdue()
        {
            var response = await _scheduleService.GetOverdueAsync();
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceScheduleDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<MaintenanceScheduleDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.ConvertAll(e => e.ErrorMessage)));
            }

            var userId = User?.Identity?.Name ?? "System";
            var response = await _scheduleService.CreateAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaintenanceScheduleDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var userId = User?.Identity?.Name ?? "System";
            var response = await _scheduleService.UpdateAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _scheduleService.DeleteAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteMaintenance(Guid id, [FromBody] decimal actualCost)
        {
            var userId = User?.Identity?.Name ?? "System";
            var response = await _scheduleService.CompleteMaintenanceAsync(id, actualCost, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}