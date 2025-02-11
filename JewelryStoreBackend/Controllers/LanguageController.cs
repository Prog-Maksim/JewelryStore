using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersionNeutral]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LanguageController: ControllerBase
{
    /// <summary>
    /// Возвращает все доступные языки приложения
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     GET /api/{version}/Language/languages
    /// 
    /// </remarks>
    /// <response code="200">Возвращает список доступных языков для использования</response>
    [AllowAnonymous]
    [HttpGet("languages")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public IActionResult GetLanguages()
    {
        return Ok(new[] { "RU/ru", "EN/en" });
    }
}