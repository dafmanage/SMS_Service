using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public class TokenBlacklistHandler : AuthorizationHandler<TokenBlacklistRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _cache;

    public TokenBlacklistHandler(IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenBlacklistRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            string token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (!string.IsNullOrEmpty(token))
            {
                var blacklisted = await _cache.GetStringAsync($"BlacklistedToken_{token}");
                if (string.IsNullOrEmpty(blacklisted))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            context.Fail();
        }
    }
}

public class TokenBlacklistRequirement : IAuthorizationRequirement { }