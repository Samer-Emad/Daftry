using System.Collections.Generic;
using Daftry.Models; // للتأكد من قراءة كلاسات الـ Order والـ Payment

namespace Daftry.ViewModels
{
    public class CustomerStatementVM
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalPurchases { get; set; }

        public decimal TotalPayments { get; set; }

        public decimal Balance => TotalPurchases - TotalPayments;

        // ====== الخواص الجديدة المضافة للتقارير والتفاصيل ======

        // نوع الفلتر الحالي (day / week / month / all)
        public string SelectedPeriod { get; set; } = "all";

        // مجموع الفواتير في الفترة المتفلترة دي بس
        public decimal PeriodPurchasesTotal { get; set; }

        // قائمة الفواتير المتراكمة لعرضها في الجدول
        public List<Order> Orders { get; set; } = new();

        // سجل المدفوعات والتصفيات السابقة
        public List<Payment> Payments { get; set; } = new();
    }
}