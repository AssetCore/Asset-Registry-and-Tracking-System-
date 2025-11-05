using Microsoft.AspNetCore.Mvc;
using AsgardeoMicroservice.Models;
using AsgardeoMicroservice.Services;

namespace AsgardeoMicroservice.Controllers
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class MeController : ControllerBase
    {
        private readonly IMeService _userService;
        private readonly ILogger<MeController> _logger;

        public MeController(IMeService userService, ILogger<MeController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get current logged-in user details
        /// Endpoint: GET /api/user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ScimUser>> GetCurrentUser([FromQuery] string? attributes = null, [FromQuery] string? excludedAttributes = null)
        {
            try
            {
                _logger.LogInformation("Fetching current user details");
                
                var user = await _userService.GetCurrentUserAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current user details");
                return StatusCode(500, "An error occurred while fetching user details");
            }
        }

        /// <summary>
        /// Update current logged-in user details
        /// Endpoint: PUT /api/user
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ScimUser>> UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                _logger.LogInformation("Updating user details");
                
                var user = await _userService.UpdateCurrentUserAsync(request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current user details");
                return StatusCode(500, "An error occurred while updating user details");
            }
        }

        /// <summary>
        /// Patch current logged-in user details
        /// Endpoint: PATCH /api/user
        /// </summary>
        [HttpPatch]
        public async Task<ActionResult<ScimUser>> PatchCurrentUser([FromBody] PatchUserRequest request)
        {
            try
            {
                _logger.LogInformation("Patching user details");
                
                var user = await _userService.PatchCurrentUserAsync(request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching current user details");
                return StatusCode(500, "An error occurred while patching user details");
            }
        }
    }
}
