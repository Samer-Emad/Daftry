using System.ComponentModel.DataAnnotations;

namespace Daftry.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "إسم العميل مطلوب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم التليفون مطلوب")]
        public string Phone { get; set; }

        // العلاقات
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
