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
    public class WarrantyInfoController : ControllerBase
    {
        private readonly IWarrantyInfoService _warrantyService;
        private readonly IValidator<CreateWarrantyInfoDto> _createValidator;

        public WarrantyInfoController(
            IWarrantyInfoService warrantyService,
            IValidator<CreateWarrantyInfoDto> createValidator)
        {
            _warrantyService = warrantyService;
            _createValidator = createValidator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _warrantyService.GetByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("asset/{assetId}")]
        public async Task<IActionResult> GetByAssetId(Guid assetId)
        {
            var response = await _warrantyService.GetByAssetIdAsync(assetId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringWarranties([FromQuery] int days = 30)
        {
            if (days <= 0)
                return BadRequest("Days parameter must be greater than 0");

            var response = await _warrantyService.GetExpiringWarrantiesAsync(days);
            return Ok(response);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveWarranties()
        {
            var response = await _warrantyService.GetActiveWarrantiesAsync();
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWarrantyInfoDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<WarrantyInfoDto>.ErrorResponse(
                    "Validation failed",
                    validationResult.Errors.ConvertAll(e => e.ErrorMessage)));
            }

            var userId = User?.Identity?.Name ?? "System";
            var response = await _warrantyService.CreateAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarrantyInfoDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID mismatch");

            var userId = User?.Identity?.Name ?? "System";
            var response = await _warrantyService.UpdateAsync(dto, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _warrantyService.DeleteAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}