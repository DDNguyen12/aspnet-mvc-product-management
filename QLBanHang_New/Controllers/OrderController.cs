using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBanHang_New.Data;
using QLBanHang_New.Models;
using QLBanHang_New.Helpers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    //ROLE
    private int GetRole()
    {
        return AuthHelper.GetRole(HttpContext);
    }

    private bool IsStaff()
    {
        return GetRole() <= 3;
    }

    private bool IsAdmin()
    {
        return GetRole() <= 2; 
    }

    // DANH SÁCH
    public IActionResult Index()
    {
        if (!IsStaff())
            return RedirectToAction("Login", "Auth");

        var orders = _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders);
    }

    // CHI TIẾT
    public IActionResult Detail(int id)
    {
        if (!IsStaff())
            return RedirectToAction("Login", "Auth");

        var order = _context.Orders
            .Include(o => o.User)
            .FirstOrDefault(o => o.OrderId == id);

        if (order == null)
            return NotFound();

        var details = _context.OrderDetails
            .Where(d => d.OrderId == id)
            .Join(_context.Products,
                d => d.ProductId,
                p => p.ProductId,
                (d, p) => new OrderDetailViewModel
                {
                    ProductName = p.ProductName,
                    Quantity = d.Quantity,
                    Price = d.Price
                })
            .ToList();

        ViewBag.Order = order;

        return View(details);
    }

    // DUYỆT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Approve(int id)
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Auth");

        var order = _context.Orders.Find(id);

        if (order == null)
            return NotFound();

        order.Status = "Đã xử lý";
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // HỦY
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        if (!IsAdmin())
            return RedirectToAction("Login", "Auth");

        var order = _context.Orders.Find(id);

        if (order == null)
            return NotFound();

        order.Status = "Đã hủy";
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}