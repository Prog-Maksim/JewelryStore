using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Models.Request.Rating;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Rating;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RatingController(ApplicationContext context,  IConnectionMultiplexer redis, MessageRepository repository, ProductRepository _product): ControllerBase
{
    /// <summary>
    /// Создает новый комментарий к товару
    /// </summary>
    /// <param name="message">Информация о комментарии</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     POST /api/{version}/Rating/comment
    ///     {
    ///           "text": "ваш большой комментарий, минимум 50 символов",
    ///           "rating": int,
    ///           "productId": string
    ///     }
    ///
    ///Требуется передать JWT access токен в заголовке Authorization
    ///
    ///     "Authorization": "Bearer {refresh токен}"
    /// 
    /// </remarks>
    /// <response code="200">Успешное создание комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPost("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreatemessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddNewComment(NewMessage message)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);

        var product = await _product.GetProductByIdAsync("RU/ru", message.SKU);

        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товар не найден",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        Message newMessage = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            PersonId = dataToken.UserId,
            ProdutId = message.SKU,
            ReplyMessageId = message.replyMessageId,

            Text = message.text,
            Rating = message.rating,
            Timestamp = DateTime.Now,
            Status = MessageStatus.Sent
        };
        
        string messageId = await repository.AddMessageAsync(newMessage);
        
        return Ok(new SuccessfulCreatemessage
        {
            Success = true,
            Message = "Комментарий успешно добавлен",
            CommentId = messageId
        });
    }
    
    /// <summary>
    /// Обновляет комментарий к товару
    /// </summary>
    /// <param name="message">Информация о комментарии</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     PUT /api/{version}/Rating/comment
    ///     {
    ///           "newText": "ваш новый большой комментарий, минимум 50 символов",
    ///           "newRating": int,
    ///           "productId": string,
    ///           "messageId": string
    ///     }
    ///
    ///Требуется передать JWT access токен в заголовке Authorization
    ///
    ///     "Authorization": "Bearer {refresh токен}"
    /// 
    /// </remarks>
    /// <response code="200">Успешное обновление комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Отказано в доступе</response>
    /// <response code="404">Сообщение не найдено</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPut("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreatemessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(UpdateMessage message)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        
        var result = await repository.GetMessageByIdAsync(message.SKU, message.messageId);

        if (result == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Комментарий не найден",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }

        if (result.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
            await repository.UpdateMessageAsync(message.SKU, message.messageId, message.newText, message.newRating);
        else
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(new SuccessfulCreatemessage
        {
            Success = true,
            Message = "Комментарий успешно обновлен",
            CommentId = message.messageId
        });
    }
    
    /// <summary>
    /// Удаляет комментарий к товару
    /// </summary>
    /// <param name="SKU">Идентификатор товара</param>
    /// <param name="messageId">Идентификатор комментария</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     DELETE /api/{version}/Rating/comment?productId={идентификатор продукта}&amp;messageId={идентификатор комментария}
    ///
    ///Требуется передать JWT access токен в заголовке Authorization
    ///
    ///     "Authorization": "Bearer {refresh токен}"
    /// 
    /// </remarks>
    /// <response code="200">Успешное удаление комментария</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Отказано в доступе</response>
    /// <response code="404">Сообщение не найдено</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpDelete("comment")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(SuccessfulCreatemessage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment([Required][FromQuery] string SKU, [Required][FromQuery] string messageId)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var message = await repository.GetMessageByIdAsync(SKU, messageId);

        if (message == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Комментарий не найден",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        if (message.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
            await repository.DeleteMessageAsync(SKU, messageId);
        else
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(new SuccessfulCreatemessage
        {
            Success = true,
            Message = "Комментарий успешно удален",
            CommentId = messageId
        });
    }
    
    /// <summary>
    /// Возвращает все комментарии для товара
    /// </summary>
    /// <param name="SKU">Идентификатор товара</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     GET /api/{version}/Rating/comments?productId={идентификатор продукта}
    ///
    /// </remarks>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("comments")]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments([Required][FromQuery] string SKU)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var comments = await repository.GetMessagesByProductIdAsync(SKU);
            return Ok(comments);
        }
        
        var token = authHeader["Bearer ".Length..];

        var dataToken = JwtController.GetJwtTokenData(token);
        var database = redis.GetDatabase();

        if (await JwtController.ValidateAccessJwtToken(database, dataToken))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var comments1 = await repository.GetMessagesByProductIdAsync(SKU);

        foreach (var comment in comments1)
        {
            if (comment.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
                comment.SendBy = PersonStatus.SendByYou;
            else
                comment.SendBy = PersonStatus.SendByAnother;
        }
        
        return Ok(comments1);
    }

    /// <summary>
    /// Возвращает кол-во комментариев у товара
    /// </summary>
    /// <param name="SKU">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("count-comments")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountComments([Required] [FromQuery] string SKU)
    {
        return Ok(await repository.GetMessageCountByProductIdAsync(SKU));
    }
    
    /// <summary>
    /// Возвращает рейтинг товара
    /// </summary>
    /// <param name="SKU">Идентификатор товара</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     GET /api/{version}/Rating/rating?productId={идентификатор продукта}
    ///
    /// </remarks>
    /// <response code="200">Успешно</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("rating")]
    [ProducesResponseType(typeof(ProductRating), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRating([Required][FromQuery] string SKU)
    {
        var ratings = await repository.GetAllRatingsAsync(SKU);

        if (ratings.Count == 0)
            return Ok(0.0);

        var averageRating = Math.Round(ratings.Average(), 2);

        return Ok(new ProductRating
        {
            Rating = averageRating,
            CustomersCount = ratings.Count
        });
    }
    
    ///  <summary>
    ///  Возвращает кол-во лайков у товара
    ///  </summary>
    ///  <param name="SKU">Идентификатор товара</param>
    ///  <remarks>
    /// Пример запроса:
    ///  
    ///      GET /api/{version}/Rating/like?productId={идентификатор товара}
    ///  
    ///  </remarks>
    ///  <returns>Кол-во лайков у товара</returns>
    ///  <response code="200">Успешно</response>
    ///  <response code="404">Искомый товар не найден</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpGet("like")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLikes([Required][FromQuery] string SKU)
    {
        var product = await _product.GetProductByIdAsync("RU/ru", SKU);
        
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
        
        return Ok(product.likes);
    }

    /// <summary>
    /// Устанавливает или снимает лайк на товар
    /// </summary>
    /// <param name="SKU">Идентификатор товара</param>
    /// <returns></returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     POST /api/{version}/Rating/like?productId={идентификатор продукта}
    ///
    ///Требуется передать JWT access токен в заголовке Authorization
    ///
    ///     "Authorization": "Bearer {refresh токен}"
    /// 
    /// </remarks>
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
    public async Task<IActionResult> Like([Required][FromQuery] string SKU)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var product = await _product.GetProductByIdAsync("RU/ru", SKU);

        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный товар не найден!",
                StatusCode = 404,
                Error = "Not Found"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var result = await context.UsersLike.FirstOrDefaultAsync(l => l.ProductId == SKU && l.PersonId == dataToken.UserId);

        if (result == null)
        {
            product.likes += 1;

            UsersLike like = new UsersLike
            {
                ProductId = SKU,
                PersonId = dataToken.UserId
            };
            
            await _product.UpdateProductAsync(SKU, product);
            
            await context.UsersLike.AddAsync(like);
            await context.SaveChangesAsync();
            
            return Ok(new StateLike
            {
                Success = true,
                Message = "Лайк успешно добавлен",
                IsLiked = true
            });
        }
        
        product.likes -= 1;
        await _product.UpdateProductAsync(SKU, product);
        
        context.UsersLike.Remove(result);
        await context.SaveChangesAsync();
            
        return Ok(new StateLike
        {
            Success = true,
            Message = "Лайк успешно удален",
            IsLiked = false
        });
    }

    ///  <summary>
    ///  Указывает, ставил ли пользователь лайк на товар
    ///  </summary>
    ///  <param name="SKU">Идентификатор товара</param>
    ///  <returns></returns>
    ///  <remarks>
    /// Пример запроса:
    ///  
    ///      GET /api/{version}/Rating/set-like-user?productId={идентификатор продукта}
    /// 
    /// Требуется передать JWT access токен в заголовке Authorization
    /// 
    ///      "Authorization": "Bearer {refresh токен}"
    ///  
    ///  </remarks>
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
    public async Task<IActionResult> SetLikeUser([Required][FromQuery] string SKU)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var product = await _product.GetProductByIdAsync("RU/ru", SKU);

        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный товар не найден!",
                StatusCode = 404,
                Error = "Not Found"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var result = await context.UsersLike.FirstOrDefaultAsync(l => l.ProductId == SKU && l.PersonId == dataToken.UserId);

        if (result != null)
        {
            return Ok(new StateLike
            {
                Success = true,
                Message = "Лайк установлен",
                IsLiked = true
            });
        }
        
        return Ok(new StateLike
        {
            Success = true,
            Message = "Лайк не установлен",
            IsLiked = false
        });
    }
}