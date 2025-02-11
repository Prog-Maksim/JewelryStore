using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Request.Profile;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProfileController(ApplicationContext context, IConnectionMultiplexer redis): ControllerBase
{
    private readonly PasswordHasher<Person> _passwordHasher = new();
    
    /// <summary>
    /// Выдает информацию о пользователе
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("person")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(PersonInform), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonInform()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == dataToken.UserId);
        
        return Ok(new PersonInform
        {
            Name = user.Name,
            Surname = user.Surname,
            Patronymic = user.Patronymic,
            Email = user.Email,
            Adress = user.Adress
        });
    }
    
    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    /// <param name="updatePassword"></param>
    /// <returns></returns>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var database = redis.GetDatabase();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == dataToken.UserId);

        if (_passwordHasher.VerifyHashedPassword(user, user.Password, updatePassword.OldPassword) !=
            PasswordVerificationResult.Success)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Пароль не верен!",
                ErrorCode = 423,
                Error = "Locked"
            };

            return StatusCode(error.ErrorCode, error);
        }

        user.PasswordVersion += 1;
        user.Password = _passwordHasher.HashPassword(user, updatePassword.NewPassword);
        
        var tokens = await context.Tokens.Where(p => p.PersonId == dataToken.UserId).ToListAsync();

        await JwtController.AddTokensToBan(database, dataToken.UserId, tokens.Select(p => p.AccessToken).ToList());

        context.RemoveRange(tokens);
        await context.SaveChangesAsync();

        return Ok("Пароль успешно обновлен!");
    }
}