using Microsoft.AspNetCore.Mvc;
using QLBanHang_New.Data;
using QLBanHang_New.Models;
using System.Linq;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===== LOGIN =====
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        // kiểm tra rỗng
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
            ViewBag.Username = username; // 🔥 giữ lại input
            return View();
        }

        var user = _context.Users
            .FirstOrDefault(x => x.Username != null &&
                                 x.Username == username &&
                                 x.Password == password);

        if (user == null)
        {
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            ViewBag.Username = username; // 🔥 giữ lại input
            return View();
        }

        // lưu session
        HttpContext.Session.SetString("User", user.Username ?? "");
        HttpContext.Session.SetString("Role", user.RoleId.ToString());

        return RedirectToAction("Index", "Product");
    }

    // ===== REGISTER =====
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(User u, string confirmPassword)
    {
        // kiểm tra rỗng
        if (string.IsNullOrWhiteSpace(u.Username) || string.IsNullOrWhiteSpace(u.Password))
        {
            ViewBag.Error = "Không được để trống";
            return View(u);
        }

        // confirm password
        if (u.Password != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu không khớp";
            return View(u);
        }

        // check trùng
        var exist = _context.Users.Any(x => x.Username == u.Username);
        if (exist)
        {
            ViewBag.Error = "Username đã tồn tại";
            return View(u);
        }

        u.RoleId = 4; // khách hàng

        _context.Users.Add(u);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    // ===== LOGOUT =====
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}