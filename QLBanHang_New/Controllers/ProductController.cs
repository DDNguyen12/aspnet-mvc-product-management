using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBanHang_New.Data;
using QLBanHang_New.Models;
using QLBanHang_New.Helpers;
using System.Linq;

namespace QLBanHang_New.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== CHECK ROLE =====
        private bool IsStaff()
        {
            if (!AuthHelper.IsLoggedIn(HttpContext)) return false;
            int role = AuthHelper.GetRole(HttpContext);
            return role <= 2;
        }

        // ================= DANH SÁCH (SEARCH + FILTER) =================
        public IActionResult Index(string search, int? categoryId)
        {
            var products = _context.Products
                                   .AsNoTracking()
                                   .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    (p.ProductName ?? "").Contains(search));
            }

            // 🎯 FILTER CATEGORY
            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // 🔥 LOAD CATEGORY (QUAN TRỌNG)
            ViewBag.Categories = _context.Categories
                                        .AsNoTracking()
                                        .ToList();

            return View(products.ToList());
        }

        // ================= CHI TIẾT =================
        public IActionResult Detail(int id)
        {
            var product = _context.Products
                                  .AsNoTracking()
                                  .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Auth");

            // 🔥 load category cho dropdown
            ViewBag.Categories = _context.Categories.ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product p)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(p);
            }

            _context.Products.Add(p);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= EDIT =================
        public IActionResult Edit(int id)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Auth");

            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            // 🔥 load category
            ViewBag.Categories = _context.Categories.ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product p)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(p);
            }

            var existing = _context.Products
                                   .FirstOrDefault(x => x.ProductId == p.ProductId);

            if (existing == null)
                return NotFound();

            // update field
            existing.ProductName = p.ProductName;
            existing.Price = p.Price;
            existing.Stock = p.Stock;
            existing.CategoryId = p.CategoryId;
            existing.ImageUrl = p.ImageUrl;
            existing.Description = p.Description;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Auth");

            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}