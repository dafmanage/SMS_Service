using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Implementation.DTOS.Authentication;
using Implementation.Interfaces.Authentication;
using Implementation.Helper;
using System.Net;
using IntegratedInfrustructure.Model.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;


namespace ERPSystems.Controllers.Authentication
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        IAuthenticationService _authenticationService;

        private SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;


        public AuthenticationController(IAuthenticationService authenticationService, SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache)
        {
            _authenticationService = authenticationService;
            _signInManager = signInManager;
            _cache = cache;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.Login(loginDto));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        public async Task<ActionResult<ResponseMessage>> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await _cache.RemoveAsync($"UserSession_{userId}");
                // Remove the old session from the cache

            }
            await _signInManager.SignOutAsync();
            return Ok(new ResponseMessage
            {
                Success = true,
                ErrorCode = 0,
                Message = "Logged out successfully"
            });
        }


        [HttpGet]
        [ProducesResponseType(typeof(UserListDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserList()
        {
            return Ok(await _authenticationService.GetUserList());
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRoleCategory()
        {
            return Ok(await _authenticationService.GetRoleCategory());
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetNotAssignedRole(string userId)
        {
            return Ok(await _authenticationService.GetNotAssignedRole(userId));
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDropDown), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssignedRoles(string userId)
        {
            return Ok(await _authenticationService.GetAssignedRoles(userId));
        }


        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddUser([FromBody] AddUSerDto addUSer)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.AddUser(addUSer));
            }
            else
            {
                return BadRequest();
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AssingRole(UserRoleDto userRole)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.AssignRole(userRole));
            }
            else
            {
                return BadRequest();
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RevokeRole(UserRoleDto userRole)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.RevokeRole(userRole));
            }
            else
            {
                return BadRequest();
            }
        }

        //[HttpPost]
        //[ProducesResponseType(typeof(ResponseMessage), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> ChangeStatusOfUser(string userId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        return Ok(await _authenticationService.ChangeStatusOfUser(userId));
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _authenticationService.ChangePassword(model));
            }
            else
            {
                return BadRequest();
            }
        }


    }
}
