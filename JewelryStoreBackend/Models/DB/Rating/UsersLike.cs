using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.DB.Rating;

public class UsersLike
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользоватедя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [MaxLength(20)]
    public required string ProductId { get; set; }
}