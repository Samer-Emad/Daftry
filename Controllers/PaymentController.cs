using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Daftry.Data;
using Daftry.Models;

namespace Daftry.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly SuperMarketDbContext _context;

        public PaymentsController(SuperMarketDbContext context)
        {
            _context = context;
        }

        // تسجيل حركة دفع كاش - POST 
        [HttpPost]
        public async Task<IActionResult> AddPayment(int customerId, decimal amount)
        {
            var customer = await _context.Customers
                .Select(c => new {
                    c.Id,
                    CurrentDebt = c.Orders.SelectMany(o => o.Items).Sum(i => i.Quantity * i.UnitPrice)
                                  - c.Payments.Sum(p => p.Amount)
                })
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null) return NotFound();

            if (amount > customer.CurrentDebt)
            {
                TempData["ErrorMessage"] = $" خطأ: لا يمكن دفع مبلغ ({amount} جنيه) لأنه أكبر من الحساب المتبقي على العميل وهو ({customer.CurrentDebt} جنيه).";
                return RedirectToAction("Details", "Customers", new { id = customerId });
            }

            var payment = new Payment
            {
                CustomerId = customerId,
                Amount = amount,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تسجيل الدفعة النقدية وتحديث الحساب بنجاح.";
            return RedirectToAction("Details", "Customers", new { id = customerId });
        }
    }
}