namespace Daftry.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public decimal Amount { get; set; } // المبلغ المدفوع
        public DateTime PaymentDate { get; set; }
    }
}
