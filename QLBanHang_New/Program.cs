using Microsoft.EntityFrameworkCore;
using QLBanHang_New.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== SESSION =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// (Optional nhưng chuẩn)
builder.Services.AddAuthentication();

var app = builder.Build();

// ========================
// MIDDLEWARE
// ========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔥 thứ tự đúng
app.UseSession();
app.UseAuthentication(); // 🔥 thêm dòng này
app.UseAuthorization();

// ===== ROUTE =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();