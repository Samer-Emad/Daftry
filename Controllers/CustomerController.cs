using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Daftry.Data;
using Daftry.Models;
using Daftry.ViewModels;

namespace Daftry.Controllers
{
    public class CustomersController : Controller
    {
        private readonly SuperMarketDbContext _context;

        public CustomersController(SuperMarketDbContext context)
        {
            _context = context;
        }

        // شاشة عرض كل العملاء (Index)
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        // شاشة إضافة عميل جديد - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // حفظ العميل الجديد في قاعدة البيانات - POST
        [HttpPost]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Customers.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // شاشة الدفتر والتفاصيل والتقارير الحية للعميل
        public async Task<IActionResult> Details(int id, string period = "all")
        {
            var customer = await _context.Customers
                .Include(x => x.Orders).ThenInclude(x => x.Items)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null)
                return NotFound();

            // الحسابات التراكمية العامة للدفتر
            var totalPurchases = customer.Orders.SelectMany(x => x.Items).Sum(x => x.Quantity * x.UnitPrice);
            var totalPayments = customer.Payments.Sum(x => x.Amount);

            // تطبيق الفلتر الزمني على المشتريات (يومي / أسبوعي / شهري / سنوي)
            var filteredOrders = customer.Orders.AsEnumerable();
            DateTime filterDate = DateTime.MinValue;

            if (period.ToLower() == "day")
                filterDate = DateTime.Today;
            else if (period.ToLower() == "week")
                filterDate = DateTime.Today.AddDays(-7);
            else if (period.ToLower() == "month")
                filterDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            else if (period.ToLower() == "year") // 🛠️ تم التحديث: دعم فلتر السنة الحالية ليتوافق مع الفيو
                filterDate = new DateTime(DateTime.Today.Year, 1, 1);

            if (filterDate != DateTime.MinValue)
                filteredOrders = filteredOrders.Where(x => x.OrderDate >= filterDate);

            // إجمالي مشتريات الفترة المحددة فقط
            var periodPurchasesTotal = filteredOrders.SelectMany(x => x.Items).Sum(x => x.Quantity * x.UnitPrice);

            var vm = new CustomerStatementVM
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TotalPurchases = totalPurchases,
                TotalPayments = totalPayments,
                SelectedPeriod = period,
                PeriodPurchasesTotal = periodPurchasesTotal,
                Orders = filteredOrders.OrderByDescending(x => x.OrderDate).ToList(),
                Payments = customer.Payments.OrderByDescending(x => x.PaymentDate).ToList()
            };

            return View(vm);
        }

        // تعديل بيانات العميل - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // حفظ تعديل بيانات العميل - POST
        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var customerInDb = await _context.Customers.FindAsync(customer.Id);
            if (customerInDb == null)
                return NotFound();

            // ✅ تم التصليح: إسناد القيم للخصائص الصحيحة (الاسم للاسم والتليفون للتليفون)
            customerInDb.Name = customer.Name;
            customerInDb.Phone = customer.Phone;

            _context.Customers.Update(customerInDb);
            await _context.SaveChangesAsync();

            // التوجيه لصفحة الـ Details للعميل نفسه بعد الحفظ بنجاح
            return RedirectToAction(nameof(Details), new { id = customer.Id });
        }

        // حذف عميل بالكامل مع كل بياناته - POST
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            var customer = await _context.Customers
                .Include(x => x.Orders).ThenInclude(x => x.Items)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == customerId);

            if (customer == null) return NotFound();

            foreach (var order in customer.Orders)
            {
                _context.OrderItems.RemoveRange(order.Items);
            }
            _context.Orders.RemoveRange(customer.Orders);
            _context.Payments.RemoveRange(customer.Payments);

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}