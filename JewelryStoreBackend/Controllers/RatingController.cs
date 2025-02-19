using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Request.Rating;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Rating;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RatingController(
    IMessageRepository repository, 
    IProductRepository productRepository,
    ProductService productService): ControllerBase
{
    private JwtTokenData GetUserIdFromToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var dataToken = JwtController.GetJwtTokenData(token);
        return dataToken;
    }

    /// <summary>
    /// Создает новый комментарий к товару
    /// </summary>
    /// <param name="message">Информация о комментарии</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное создание комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPost("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreateMessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddNewComment(NewMessage message, string languageCode)
    {
        var dataToken = GetUserIdFromToken();
        var response = await productService.AddNewCommentAsync(dataToken.UserId, languageCode, message);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Обновляет комментарий к товару
    /// </summary>
    /// <param name="message">Информация о комментарии</param>
    /// <returns></returns>
    /// <response code="200">Успешное обновление комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Отказано в доступе</response>
    /// <response code="404">Сообщение не найдено</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPut("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreateMessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(UpdateMessage message)
    {
        var dataToken = GetUserIdFromToken();
        var response = await productService.UpdateCommentAsync(dataToken, message);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Удаляет комментарий к товару
    /// </summary>
    /// <param name="sku">Идентификатор товара</param>
    /// <param name="messageId">Идентификатор комментария</param>
    /// <returns></returns>
    /// <response code="200">Успешное удаление комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Отказано в доступе</response>
    /// <response code="404">Сообщение не найдено</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpDelete("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreateMessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment([Required][FromQuery] string sku, [Required][FromQuery] string messageId)
    {
        var dataToken = GetUserIdFromToken();
        var response = await productService.DeleteCommentAsync(dataToken, sku, messageId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Возвращает все комментарии для товара, возможна авторизация
    /// </summary>
    /// <param name="sku">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("comments")]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments([Required][FromQuery] string sku)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var comments = await repository.GetMessagesByProductIdAsync(sku);

            if (!comments.Any())
            {
                var error = new BaseResponse
                {
                    Message = "Комментарии не найдены",
                    Success = false,
                    StatusCode = 404,
                    Error = "Not found"
                };
                return StatusCode(error.StatusCode, error);
            }

            return Ok(comments);
        }
        
        var dataToken = GetUserIdFromToken();
        var (response, results) = await productService.GetAllMesageAuthorizeAsync(dataToken, sku);

        if (!response.Success)
            return StatusCode(response.StatusCode, response);

        return Ok(results);
    }

    /// <summary>
    /// Возвращает кол-во комментариев у товара
    /// </summary>
    /// <param name="sku">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("count-comments")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountComments([Required] [FromQuery] string sku)
    {
        return Ok(await repository.GetMessageCountByProductIdAsync(sku));
    }
    
    /// <summary>
    /// Возвращает рейтинг товара
    /// </summary>
    /// <param name="sku">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("rating")]
    [ProducesResponseType(typeof(ProductRating), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRating([Required][FromQuery] string sku)
    {
        return Ok(await productService.GetProductRatingAsync(sku));
    }

    ///  <summary>
    ///  Возвращает кол-во лайков у товара
    ///  </summary>
    ///  <param name="languageCode">Языковой код</param>
    ///  <param name="sku">Идентификатор товара</param>
    ///  <returns>Кол-во лайков у товара</returns>
    ///  <response code="200">Успешно</response>
    ///  <response code="404">Искомый товар не найден</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("like")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLikes([Required][FromQuery] string languageCode, [Required][FromQuery] string sku)
    {
        var product = await productRepository.GetProductByIdAsync(languageCode, sku);
        
        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный товар не найден",
                StatusCode = 404,
                Error = "Not Found"
            };
            return StatusCode(error.StatusCode, error);
        } 
        
        return Ok(product.Likes);
    }

    /// <summary>
    /// Устанавливает или снимает лайк на товар
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Успешное лайкнут</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="404">Искомый товар не найден</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPost("like")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(StateLike), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Like([Required][FromQuery] string languageCode, [Required][FromQuery] string sku)
    {
        var dataToken = GetUserIdFromToken();
        var response = await productService.ToggleLikeAsync(dataToken.UserId, languageCode, sku);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }

    ///  <summary>
    ///  Указывает, ставил ли пользователь лайк на товар
    ///  </summary>
    ///  <param name="languageCode">Языковой код</param>
    ///  <param name="sku">Идентификатор товара</param>
    ///  <returns></returns>
    ///  <response code="200">Успешно</response>
    ///  <response code="400">Невозможно определить IP адрес пользователя</response>
    ///  <response code="403">Некорректный токен</response>
    ///  <response code="404">Искомый товар не найден</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpGet("set-like-user")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(StateLike), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetLikeUser([Required][FromQuery] string languageCode, [Required][FromQuery] string sku)
    {
        var dataToken = GetUserIdFromToken();
        var response = await productService.IsProductLikedByUserAsync(dataToken.UserId, languageCode, sku);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
}