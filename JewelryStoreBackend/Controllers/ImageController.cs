using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ImageController(S3Service service): ControllerBase
{
    private const long MaxFileSize = 1 * 1024 * 1024;
    
    /// <summary>
    /// Добавляет изображение в систему
    /// </summary>
    /// <param name="file">Изображение</param>
    /// <returns></returns>
    ///  <response code="200">Успешное добавление фотографии.</response>
    ///  <response code="400">Размер фотографии превышает 1 мб или формат файла не поддерживается.</response>
    ///  <response code="418">Не удалось сохранить изображение из-за внутренней ошибки.</response>
    ///  <response code="500">Произошла неизвестная ошибка.</response>
    [Authorize(Roles = "manager,admin")]
    [MapToApiVersion("1.0")]
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status418ImATeapot)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddImage([Required]IFormFile file)
    {
        if (file.Length > MaxFileSize)
        {
            BaseResponse errorFailed = new BaseResponse
            {
                Message = "Файл слишком большой",
                StatusCode = 400,
                Error = "Bad Request",
                Success = false
            };
            return StatusCode(errorFailed.StatusCode, errorFailed);
        }
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            BaseResponse errorFailed = new BaseResponse
            {
                Message = "Неверный тип файла. Допускаются только .png и .jpg",
                StatusCode = 400,
                Error = "Bad Request",
                Success = false
            };
            return StatusCode(errorFailed.StatusCode, errorFailed);
        }

        try
        {
            var fileId = await service.UploadFileToS3Async(file, Guid.NewGuid().ToString());

            return Ok(new
            {
                Message = "Изображение успешно загружено!",
                ProductId = fileId
            });
        }
        catch (FileLoadException)
        {
            BaseResponse errorFailed = new BaseResponse
            {
                Message = "Не удается загрузить изображение товара",
                StatusCode = 418,
                Error = "I’m a teapot",
                Success = false
            };
            return StatusCode(errorFailed.StatusCode, errorFailed);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            BaseResponse errorFailed = new BaseResponse
            {
                Message = "Произошла неизвестная ошибка",
                StatusCode = 500,
                Error = "Internal Server Error",
                Success = false
            };
            return StatusCode(errorFailed.StatusCode, errorFailed);
        }
    }
    
    /// <summary>
    /// Удаляет изображение из системы
    /// </summary>
    /// <param name="fileId">Идентификатор файла</param>
    /// <returns></returns>
    ///  <response code="200">Успешное Удаление фотографии.</response>
    ///  <response code="404">Удаляемое изображение не найдено.</response>
    ///  <response code="418">Не удалось удалить изображение из-за внутренней ошибки.</response>
    ///  <response code="500">Произошла неизвестная ошибка.</response>
    [Authorize(Roles = "manager,admin")]
    [HttpDelete("image")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status418ImATeapot)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteImage([Required] string fileId)
    {
        try
        {
            await service.DeleteFileFromS3Async(fileId);

            return Ok(new
            {
                Message = "Изображение успешно удалено!",
                ProductId = fileId
            });
        }
        catch (FileNotFoundException errorException)
        {
            var error = new BaseResponse
            {
                Message = errorException.Message,
                StatusCode = 404,
                Error = "Not Found",
                Success = false
            };
            return StatusCode(error.StatusCode, error);
        }
        catch (FileLoadException)
        {
            var error = new BaseResponse
            {
                Message = "Не удалось удалить файл",
                StatusCode = 418,
                Error = "I am teapot",
                Success = false
            };
            return StatusCode(error.StatusCode, error);
        }
        catch (Exception)
        {
            var error = new BaseResponse
            {
                Message = "Произошла ошибка при удалении файла",
                StatusCode = 500,
                Error = "Internal Server Error",
                Success = false
            };
            return StatusCode(error.StatusCode, error);
        }
    }
}