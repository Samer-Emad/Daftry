using Daftry.Data;
using Microsoft.EntityFrameworkCore;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
// تفعيل رخصة QuestPDF المجتمعية للمشروع بالكامل

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SuperMarketDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// تأكد أن السطر ده موجود فوق عشان يفعل الـ Attributes لو مستخدمها
app.MapControllers();

// الـ Route الافتراضي للمشروع بالكامل
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Customers}/{action=Index}/{id?}");

app.Run();