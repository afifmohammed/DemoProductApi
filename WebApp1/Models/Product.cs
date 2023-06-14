using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models;

public class Product
{
    [Required]
    [Range(1,1000)]
    public int? Id { get; set; }
    [Required]
    [StringLength(20)]
    public string? Name { get; set; }
    
    [Required]
    [StringLength(50)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1000, 3000)]
    public int? Price { get; set; }
}