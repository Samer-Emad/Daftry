using System.Collections.Generic;

namespace Daftry.ViewModels
{
    public class CreateOrderVM
    {
        public int CustomerId { get; set; }

        // ضفنا اسم العميل عشان يظهر فوق في شاشة البيع
        public string CustomerName { get; set; } = string.Empty;

        // غيرنا الاسم لـ SelectedItems لتطابق الكنترولر والـ View
        public List<OrderItemVM> SelectedItems { get; set; } = new();
    }

    public class OrderItemVM
    {
        // بدل الـ ProductId، هنخليها اسم المنتج كتابة يدوي
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}