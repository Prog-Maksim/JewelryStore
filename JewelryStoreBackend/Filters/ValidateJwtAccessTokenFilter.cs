using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Repository;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JewelryStoreBackend.Filters;

public class ValidateJwtAccessTokenFilter: IAsyncActionFilter
{
    private readonly AuthorizationService _authService;
    
    public ValidateJwtAccessTokenFilter(AuthorizationService authService)
    {
        _authService = authService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        var authHeader = request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Result = new ObjectResult(new BaseResponse
            {
                Success = false,
                Message = "Токен отсутствует или неверного формата",
                StatusCode = 403,
                Error = "Forbidden"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        var database = _authService.GetRedisDatabase();

        if (await _authService.ValidateAccessJwtTokenAsync(database, dataToken))
        {
            context.Result = new ObjectResult(new BaseResponse
            {
                Success = false,
                Message = "Отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}