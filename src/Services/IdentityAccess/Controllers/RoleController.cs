using AsgardeoMicroservice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace AsgardeoMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IAsgardeoService _asgardeoService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IAsgardeoService asgardeoService, ILogger<RoleController> logger)
        {
            _asgardeoService = asgardeoService;
            _logger = logger;
        }

        [HttpPatch("auditor")]
        public async Task<IActionResult> PatchAuditorRole([FromBody] JsonElement patchRequest)
        {
            try
            {
                _logger.LogInformation("Patching auditor role");
                var content = await _asgardeoService.PatchAuditorRoleAsync(patchRequest);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching auditor role");
                return StatusCode(500, "An error occurred while patching the auditor role");
            }
        }

        [HttpPatch("auditUser")]
        public async Task<IActionResult> PatchAuditUserRole([FromBody] JsonElement patchRequest)
        {
            try
            {
                _logger.LogInformation("Patching audit user role");
                var content = await _asgardeoService.PatchAuditUserRoleAsync(patchRequest);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching audit user role");
                return StatusCode(500, "An error occurred while patching the auditor role");
            }
        }
    }
}
