using EComApi.Entity.DTO.Security;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace EComApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISecurityService _securityService;

        public AccountController(ISecurityService securityService)
        {
            _securityService = securityService;
        }
        // GET: api/<AccountController1>
        [HttpPost("Login")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto login)
        {
            var result = await _securityService.Login(login);
            return Ok(result);
        }
        [HttpPost("Register")]

        public async Task<IActionResult> Register([FromBody] RequestDto request)
        {
            var result = await _securityService.Register(request);

            return Ok(result);
        }
    }
}
