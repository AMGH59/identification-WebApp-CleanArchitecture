using IdentificationService.API.DTO;
using IdentificationService.API.Filters;
using IdentificationService.Application.Constants;
using IdentificationService.Application.Interfaces;
using IdentificationService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentificationService.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidateAccount]
    public class AccountController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AccountController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromForm] LoginDTO model)
        {
            try
            {
                var loginModel = new LoginModel(model.Username, model.Password);
                var token = await _identityService.LoginUserAsync(model.Username, model.Password);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> Create([FromForm] UserDTO model)
        {
            try
            {
                var userModel = new UserModel(model.Username, model.Password, model.Roles);
                var user = await _identityService.CreateUserAsync(model.Username, model.Password, model.Roles);

                return Ok(new { user });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
