using JewelryStoreBackend.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JewelryStoreBackend.Filters;

public class ValidateUserIpFilter: IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        string? userIp = request.Headers["X-Forwarded-For"].FirstOrDefault() 
                         ?? context.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(userIp))
        {
            context.Result = new BadRequestObjectResult(new BaseResponse
            {
                Success = false,
                Message = "Не удалось определить IP-адрес пользователя",
                StatusCode = 400,
                Error = "Bad Request"
            });
            return;
        }

        await next();
    }

}