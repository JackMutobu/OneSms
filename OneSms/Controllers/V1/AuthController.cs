using Microsoft.AspNetCore.Mvc;
using OneSms.Contracts.V1;
using OneSms.Contracts.V1.Requests;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using OneSms.Services;
using System;
using System.Threading.Tasks;

namespace OneSms.Controllers.V1
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAthenticationService _authenticationService;
        public AuthController(IAthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost(ApiRoutes.Auth.App)]
        public IActionResult Authenticate(ApiAuthRequest authRequest)
        {
            var authResult = _authenticationService.Authenticate(new Guid(authRequest.AppId), new Guid(authRequest.AppSecret));
            return GetAuthResponse(authResult);
        }

        [HttpPost(ApiRoutes.Auth.Server)]
        public IActionResult Authenticate(ServerAuthRequest authRequest)
        {
            var authResult = _authenticationService.Authenticate(new Guid(authRequest.ServerKey), authRequest.Secret);
            return GetAuthResponse(authResult);
        }

        [HttpPost(ApiRoutes.Auth.User)]
        public async Task<IActionResult> Authenticate(UserAuthRequest authRequest)
        {
            var authResult = await _authenticationService.Authenticate(authRequest.UserName, authRequest.Password);
            return GetAuthResponse(authResult);
        }

        private IActionResult GetAuthResponse(AuthenticationResult authResult)
        {
            if (authResult.Success)
                return Ok(new AuthSuccessResponse { Token = authResult.Token });
            return BadRequest(new AuthFailedResponse { Errors = authResult.Errors });
        }

    }
}
