using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Daftry.Data;
using Daftry.Models;
using Daftry.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Daftry.Controllers
{
    public class OrdersController : Controller
    {
        private readonly SuperMarketDbContext _context;

        public OrdersController(SuperMarketDbContext context)
        {
            _context = context;
        }

        // فتح شاشة البيع لعميل معين - GET
        [HttpGet]
        public async Task<IActionResult> Create(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return NotFound();

            return View(new CreateOrderVM { CustomerId = customerId, CustomerName = customer.Name });
        }

        // حفظ الفاتورة اليدوية وتجميعها تلقائياً - POST
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderVM model)
        {
            var validItems = model.SelectedItems
                .Where(x => x.Quantity > 0 && !string.IsNullOrWhiteSpace(x.ProductName))
                .ToList();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "يجب إضافة منتج واحد على الأقل بكتابة الاسم والكمية الصحيحة.");
                return View(model);
            }

            string generatedSerial = DateTime.Now.ToString("yyyyMMddHHmmssff");

            var order = new Order
            {
                CustomerId = model.CustomerId,
                OrderDate = DateTime.Now,
                OrderSerialNumber = generatedSerial
            };

            foreach (var item in validItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductName = item.ProductName.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Customers", new { id = model.CustomerId });
        }

        // 1. توليد وطباعة كشف الحساب الإجمالي المطور (عرض تفاصيل كل فاتورة بالمنتجات كالفاتورة المفردة)
        // 1. توليد وطباعة كشف الحساب الإجمالي المطور (عرض تفاصيل كل فاتورة بالمنتجات كالفاتورة المفردة)
        [HttpGet]
        public async Task<IActionResult> PrintAllInvoices(int customerId)
        {
            var customer = await _context.Customers
                .Include(x => x.Orders).ThenInclude(x => x.Items)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == customerId);

            if (customer == null) return NotFound();

            var totalPurchases = customer.Orders.SelectMany(x => x.Items).Sum(x => x.Quantity * x.UnitPrice);
            var totalPayments = customer.Payments.Sum(x => x.Amount);
            var currentDebt = totalPurchases - totalPayments;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.ContentFromRightToLeft();

                    // الهيدر الرئيسي لكشف الحساب (تاريخ استخراج التقرير)
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("دفتري - كشف حساب تفصيلي").FontSize(22).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"اسم العميل: {customer.Name}").FontSize(14).Medium();
                        });
                        row.ConstantItem(180).Column(col =>
                        {
                            // 🗓️ تاريخ طباعة التقرير نفسه
                            col.Item().Text($"تاريخ طباعة التقرير: {DateTime.Now:yyyy-MM-dd hh:mm tt}").FontSize(10).AlignLeft().Bold();
                            col.Item().Text($"إجمالي الفواتير: {customer.Orders.Count} فاتورة").FontSize(11).AlignLeft();
                        });
                    });

                    // محتوى الفواتير والمنتجات
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Item().PaddingBottom(15).Text("سجل الفواتير المتراكمة وتفاصيلها:").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);

                        foreach (var order in customer.Orders.OrderByDescending(o => o.OrderDate))
                        {
                            var orderTotal = order.Items.Sum(x => x.Quantity * x.UnitPrice);

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(10).PaddingBottom(15).Column(orderCol =>
                            {
                                // رأس الفاتورة الداخلية (تم إبراز تاريخ الفاتورة هنا)
                                orderCol.Item().Row(orderRow =>
                                {
                                    orderRow.RelativeItem().Text($"فاتورة رقم: {order.OrderSerialNumber}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);

                                    // 🗓️ هنا بيطبع تاريخ الفاتورة نفسها اللي اتعملت فيه
                                    orderRow.RelativeItem().AlignCenter().Text($"تاريخ الفاتورة: {order.OrderDate:yyyy-MM-dd hh:mm tt}").FontSize(11).Bold().FontColor(Colors.Black);

                                    orderRow.RelativeItem().AlignLeft().Text($"إجمالي الفاتورة: {orderTotal} ج").Bold().FontSize(12).FontColor(Colors.Red.Medium);
                                });

                                // جدول المنتجات الخاص بهذه الفاتورة
                                orderCol.Item().PaddingTop(8).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("اسم المنتج").Bold().FontSize(10);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("الكمية").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("السعر").Bold().FontSize(10).AlignCenter();
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("الإجمالي").Bold().FontSize(10).AlignCenter();
                                    });

                                    foreach (var item in order.Items)
                                    {
                                        var itemTotal = item.Quantity * item.UnitPrice;
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.ProductName).FontSize(10);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Quantity.ToString()).FontSize(10).AlignCenter();
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{item.UnitPrice} ج").FontSize(10).AlignCenter();
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{itemTotal} ج").FontSize(10).AlignCenter();
                                    }
                                });
                            });
                        }

                        // الملخص المالي النهائي
                        column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
                        {
                            row.RelativeItem().Text($"إجمالي المشتريات: {totalPurchases} ج").FontSize(12).Bold();
                            row.RelativeItem().Text($"إجمالي المدفوعات: {totalPayments} ج").FontSize(12).Bold().FontColor(Colors.Green.Medium);
                            row.RelativeItem().Text($"الصافي المطلوب حالياً: {currentDebt} ج").FontSize(13).Bold().FontColor(Colors.Red.Medium);
                        });
                    });

                    page.Footer().AlignCenter().Text("نظام دفتري للحسابات - كشف فواتير تفصيلي").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            string customerName = customer.Name.Replace(" ", "_");
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"كشف_تفصيلي_{customerName}.pdf");
        }

        // 2. طباعة فاتورة معينة واحدة بكامل التفاصيل
        // 2. طباعة فاتورة معينة واحدة بكامل التفاصيل (مع إظهار تاريخ الفاتورة وتاريخ الطباعة الحالية)
        [HttpGet]
        public async Task<IActionResult> PrintInvoice(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null) return NotFound();

            var customer = await _context.Customers.FindAsync(order.CustomerId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.ContentFromRightToLeft();

                    // هيدر الفاتورة الرئيسي
                    page.Header().Row(row =>
                    {
                        // اليمين: عنوان الفاتورة ورقمها
                        row.RelativeItem().Text($"فاتورة مبيعات رقم: {order.OrderSerialNumber}")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Medium);

                        // الشمال: تاريخ وقت طباعة الورقة الحالية 🗓️
                        row.ConstantItem(180).Text($"تاريخ الطباعة: {DateTime.Now:yyyy-MM-dd hh:mm tt}")
                            .FontSize(10).AlignLeft().FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text($"العميل: {customer?.Name}").FontSize(12).Medium();

                        // 🗓️ تاريخ الفاتورة الأصلي (وقت حدوث عملية البيع)
                        col.Item().Text($"تاريخ الفاتورة: {order.OrderDate:yyyy-MM-dd hh:mm tt}").FontSize(11).Bold();

                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(5);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("اسم المنتج").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("الكمية").Bold().AlignCenter();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("السعر").Bold().AlignCenter();
                            });

                            foreach (var item in order.Items)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.ProductName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Quantity.ToString()).AlignCenter();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.UnitPrice} ج").AlignCenter();
                            }
                        });
                    });

                    var total = order.Items.Sum(x => x.Quantity * x.UnitPrice);
                    page.Footer().PaddingTop(10).AlignLeft().Text($"إجمالي الفاتورة: {total} جنيه").FontSize(14).Bold().FontColor(Colors.Red.Medium);
                });
            });

            var pdfBytes = document.GeneratePdf();

            string safeCustomerName = (customer?.Name ?? "عميل_غير_معروف").Replace(" ", "_");
            string formattedDate = order.OrderDate.ToString("yyyy-MM-dd_hh-mmtt");

            string fileName = $"فاتورة_{safeCustomerName}_{formattedDate}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // 3. طباعة فواتير فترة محددة (يوم، أسبوع، شهر، سنة) بتفاصيل المنتجات
        // 3. طباعة فواتير فترة محددة (يوم، أسبوع، شهر، سنة) بتفاصيل المنتجات والتواريخ
        [HttpGet]
        public async Task<IActionResult> PrintPeriodInvoices(int customerId, string period)
        {
            var customer = await _context.Customers
                .Include(x => x.Orders).ThenInclude(x => x.Items)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == customerId);

            if (customer == null) return NotFound();

            var targetDate = DateTime.Now;
            var filteredOrders = customer.Orders.AsEnumerable();

            string periodNameArabic = "كل المشتريات";

            switch (period?.ToLower())
            {
                case "day":
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.Date == targetDate.Date);
                    periodNameArabic = "تقرير_اليوم";
                    break;
                case "week":
                    filteredOrders = filteredOrders.Where(o => o.OrderDate >= targetDate.AddDays(-7));
                    periodNameArabic = "تقرير_الأسبوع";
                    break;
                case "month":
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.Month == targetDate.Month && o.OrderDate.Year == targetDate.Year);
                    periodNameArabic = "تقرير_الشهر";
                    break;
                case "year":
                    filteredOrders = filteredOrders.Where(o => o.OrderDate.Year == targetDate.Year);
                    periodNameArabic = "تقرير_السنة";
                    break;
            }

            var finalOrdersList = filteredOrders.OrderByDescending(o => o.OrderDate).ToList();

            var periodPurchasesTotal = finalOrdersList.SelectMany(x => x.Items).Sum(x => x.Quantity * x.UnitPrice);
            var totalPayments = customer.Payments.Sum(x => x.Amount);
            var totalAllPurchases = customer.Orders.SelectMany(x => x.Items).Sum(x => x.Quantity * x.UnitPrice);
            var currentDebt = totalAllPurchases - totalPayments;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.ContentFromRightToLeft();

                    // هيدر التقرير الرئيسي
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"دفتري - {periodNameArabic.Replace("_", " ")}").FontSize(20).Bold().FontColor(Colors.Green.Medium);
                            col.Item().Text($"اسم العميل: {customer.Name}").FontSize(14).Medium();
                        });
                        row.ConstantItem(200).Column(col =>
                        {
                            // 🗓️ تاريخ استخراج وطباعة هذا التقرير الفتري
                            col.Item().Text($"تاريخ طباعة التقرير: {DateTime.Now:yyyy-MM-dd hh:mm tt}").FontSize(10).AlignLeft().Bold();
                            col.Item().Text($"عدد فواتير الفترة: {finalOrdersList.Count} فاتورة").FontSize(11).AlignLeft();
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        if (!finalOrdersList.Any())
                        {
                            column.Item().Padding(20).Background(Colors.Grey.Lighten4).AlignCenter().Text("لا توجد فواتير مسجلة خلال هذه الفترة الحالية.").FontSize(12).Italic();
                        }

                        foreach (var order in finalOrdersList)
                        {
                            var orderTotal = order.Items.Sum(x => x.Quantity * x.UnitPrice);

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.White).Padding(10).PaddingBottom(15).Column(orderCol =>
                            {
                                // رأس الفاتورة الداخلية
                                orderCol.Item().Row(orderRow =>
                                {
                                    orderRow.RelativeItem().Text($"فاتورة رقم: {order.OrderSerialNumber}").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);

                                    // 🗓️ طباعة تاريخ ووقت الفاتورة نفسها داخل التقرير الفتري
                                    orderRow.RelativeItem().AlignCenter().Text($"تاريخ الفاتورة: {order.OrderDate:yyyy-MM-dd hh:mm tt}").FontSize(10).Bold().FontColor(Colors.Black);

                                    orderRow.RelativeItem().AlignLeft().Text($"إجمالي الفاتورة: {orderTotal} ج").Bold().FontSize(11).FontColor(Colors.Red.Medium);
                                });

                                // جدول منتجات الفاتورة
                                orderCol.Item().PaddingTop(8).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("اسم المنتج").Bold().FontSize(9);
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("الكمية").Bold().FontSize(9).AlignCenter();
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("السعر").Bold().FontSize(9).AlignCenter();
                                        header.Cell().Background(Colors.Grey.Lighten4).Padding(4).Text("الإجمالي").Bold().FontSize(9).AlignCenter();
                                    });

                                    foreach (var item in order.Items)
                                    {
                                        var itemTotal = item.Quantity * item.UnitPrice;
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.ProductName).FontSize(9);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Quantity.ToString()).FontSize(9).AlignCenter();
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{item.UnitPrice} ج").FontSize(9).AlignCenter();
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{itemTotal} ج").FontSize(9).AlignCenter();
                                    }
                                });
                            });
                        }

                        column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
                        {
                            row.RelativeItem().Text($"مشتريات الفترة الحالية: {periodPurchasesTotal} ج").FontSize(11).Bold().FontColor(Colors.Blue.Medium);
                            row.RelativeItem().Text($"إجمالي مدفوعات العميل الكلية: {totalPayments} ج").FontSize(11).Bold().FontColor(Colors.Green.Medium);
                            row.RelativeItem().Text($"الصافي المطلوب كلياً: {currentDebt} ج").FontSize(12).Bold().FontColor(Colors.Red.Medium);
                        });
                    });

                    page.Footer().AlignCenter().Text($"نظام دفتري للحسابات - {periodNameArabic.Replace("_", " ")}").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            var pdfBytes = document.GeneratePdf();
            string safeCustomerName = customer.Name.Replace(" ", "_");
            string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");

            return File(pdfBytes, "application/pdf", $"{periodNameArabic}_{safeCustomerName}_{formattedDate}.pdf");
        }

        // تعديل فاتورة معينة - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null) return NotFound();

            var customer = await _context.Customers.FindAsync(order.CustomerId);

            var vm = new CreateOrderVM
            {
                CustomerId = order.CustomerId,
                CustomerName = customer?.Name ?? "عميل غير معروف",
                SelectedItems = order.Items.Select(x => new OrderItemVM
                {
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            ViewBag.OrderId = order.Id;
            return View(vm);
        }

        // حفظ تعديل الفاتورة - POST
        [HttpPost]
        public async Task<IActionResult> Edit(int orderId, CreateOrderVM model)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null) return NotFound();

            var validItems = model.SelectedItems
                .Where(x => x.Quantity > 0 && !string.IsNullOrWhiteSpace(x.ProductName))
                .ToList();

            if (!validItems.Any())
            {
                ModelState.AddModelError("", "يجب أن تحتوي الفاتورة على منتج واحد صحيح على الأقل.");
                ViewBag.OrderId = order.Id;
                return View(model);
            }

            _context.OrderItems.RemoveRange(order.Items);

            foreach (var item in validItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductName = item.ProductName.Trim(),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            order.OrderDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Customers", new { id = order.CustomerId });
        }

        // حذف فاتورة معينة نهائياً - POST
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null) return NotFound();

            int customerId = order.CustomerId;

            _context.OrderItems.RemoveRange(order.Items);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Customers", new { id = customerId });
        }
    }
}