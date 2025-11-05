using AsgardeoMicroservice.Models;
using AsgardeoMicroservice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace AsgardeoMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IAsgardeoService _asgardeoService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAsgardeoService asgardeoService, ILogger<UsersController> logger)
        {
            _asgardeoService = asgardeoService;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] string? filter = null, [FromQuery] string? attributes = null, [FromQuery] int? startIndex = null, [FromQuery] int? count = null)
        {
            try
            {
                _logger.LogInformation("Fetching users from Asgardeo");
                var content = await _asgardeoService.GetUsersAsync(filter, attributes, startIndex, count);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, "An error occurred while fetching users");
            }
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id, [FromQuery] string? attributes = null)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID: {UserId}", id);
                var content = await _asgardeoService.GetUserByIdAsync(id, attributes);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while fetching the user");
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] JsonElement userRequest, [FromQuery] string? attributes = null)
        {
            try
            {
                _logger.LogInformation("Creating user");
                var content = await _asgardeoService.CreateUserAsync(userRequest, attributes);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] JsonElement userRequest, [FromQuery] string? attributes = null)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {UserId}", id);
                var content = await _asgardeoService.UpdateUserAsync(id, userRequest, attributes);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while updating the user");
            }
        }

        // PATCH: api/Users/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(string id, [FromBody] JsonElement patchRequest, [FromQuery] string? attributes = null)
        {
            try
            {
                _logger.LogInformation("Patching user with ID: {UserId}", id);
                var content = await _asgardeoService.PatchUserAsync(id, patchRequest, attributes);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while patching the user");
            }
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id, [FromQuery] string? attributes = null)
        {
            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", id);
                var content = await _asgardeoService.DeleteUserAsync(id, attributes);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, "An error occurred while deleting the user");
            }
        }

        // POST: api/Users/.search
        [HttpPost(".search")]
        public async Task<IActionResult> SearchUsers([FromBody] JsonElement searchRequest)
        {
            try
            {
                _logger.LogInformation("Searching users via .search endpoint");
                var content = await _asgardeoService.SearchUsersAsync(searchRequest);
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, "An error occurred while searching users");
            }
        }
    }
}
