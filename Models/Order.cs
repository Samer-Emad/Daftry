using System;
using System.Collections.Generic;

namespace Daftry.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }

        // الحقل الجديد الخاص برقم الفاتورة المميز تلقائياً
        public string OrderSerialNumber { get; set; }

        public Customer Customer { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}