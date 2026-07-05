using System.ComponentModel.DataAnnotations;

namespace Daftry.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
