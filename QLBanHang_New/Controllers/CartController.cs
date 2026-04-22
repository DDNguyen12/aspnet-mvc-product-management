using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBanHang_New.Data;
using QLBanHang_New.Models;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===== LẤY USER ID =====
    private int GetUserId()
    {
        var user = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(user)) return -1;

        var u = _context.Users.FirstOrDefault(x => x.Username == user);
        return u?.UserId ?? -1;
    }

    // ===== GIỎ HÀNG =====
    public IActionResult Index()
    {
        int userId = GetUserId();
        if (userId == -1)
            return RedirectToAction("Login", "Auth");

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>()
            };
        }

        return View(cart);
    }

    // ===== THÊM GIỎ =====
    public IActionResult Add(int id)
    {
        int userId = GetUserId();
        if (userId == -1)
            return RedirectToAction("Login", "Auth");

        var product = _context.Products.Find(id);
        if (product == null)
            return NotFound();

        var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }

        var item = _context.CartItems
            .FirstOrDefault(c => c.CartId == cart.CartId && c.ProductId == id);

        if (item != null)
        {
            item.Quantity++;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.CartId,
                ProductId = id,
                Quantity = 1
            });
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // ===== XÓA =====
    [HttpPost]
    public IActionResult Remove(int id)
    {
        var item = _context.CartItems.Find(id);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // ===== UPDATE =====
    [HttpPost]
    public IActionResult Update(int id, int quantity)
    {
        var item = _context.CartItems.Find(id);

        if (item != null && quantity > 0)
        {
            item.Quantity = quantity;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // ===== CHECKOUT VIEW =====
    public IActionResult Checkout()
    {
        int userId = GetUserId();
        if (userId == -1)
            return RedirectToAction("Login", "Auth");

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index");

        return View(cart);
    }

    // ===== CHECKOUT POST (QUAN TRỌNG NHẤT) =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(string paymentMethod)
    {
        int userId = GetUserId();
        if (userId == -1)
            return RedirectToAction("Login", "Auth");

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index");

        decimal total = cart.CartItems
            .Sum(x => x.Quantity * (x.Product?.Price ?? 0));

        // 🔥 LOGIC QUAN TRỌNG
        string status = paymentMethod == "QR" ? "Đã xử lý" : "Chờ xử lý";

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            Status = status,
            PaymentMethod = paymentMethod // 🔥 QUAN TRỌNG
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        foreach (var item in cart.CartItems)
        {
            _context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product?.Price ?? 0
            });
        }

        _context.CartItems.RemoveRange(cart.CartItems);
        _context.SaveChanges();

        return RedirectToAction("Success");
    }

    // ===== SUCCESS =====
    public IActionResult Success()
    {
        return View();
    }

    // ===== QR =====
    public IActionResult GenerateQR()
    {
        var token = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("QR_" + token, DateTime.Now.AddMinutes(5).ToString());

        return Json(new { url = "/Cart/GetQR?token=" + token });
    }

    public IActionResult GetQR(string token)
    {
        var key = "QR_" + token;
        var value = HttpContext.Session.GetString(key);

        if (value == null)
            return Unauthorized();

        var expire = DateTime.Parse(value);
        if (expire < DateTime.Now)
            return Unauthorized();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "SecureFiles", "qr.jpg");

        return File(System.IO.File.ReadAllBytes(path), "image/jpeg");
    }
}