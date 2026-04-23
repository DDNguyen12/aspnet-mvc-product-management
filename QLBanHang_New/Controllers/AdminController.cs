using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLBanHang_New.Helpers;
using QLBanHang_New.Data;
using QLBanHang_New.Models;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using System.IO;
using System.Linq;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    //  CHECK ROLE 
    private bool CheckAdmin()
    {
        int role = AuthHelper.GetRole(HttpContext);
        return role == 1;
    }

    //  INDEX 
    public IActionResult Index(string search)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        var products = _context.Products.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            products = products.Where(x =>
                (x.ProductName ?? "").Contains(search));
        }

        return View(products.ToList());
    }

    // CREATE 

    // GET CREATE
    public IActionResult Create()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        LoadCategoryDropdown();
        return View();
    }

    // POST CREATE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product p, IFormFile imageFile, string NewCategory)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        // CATEGORY 
        if (!string.IsNullOrWhiteSpace(NewCategory))
        {
            var cat = new Category { CategoryName = NewCategory };
            _context.Categories.Add(cat);
            _context.SaveChanges();

            p.CategoryId = cat.CategoryId;
        }
        else
        {
            if (p.CategoryId == 0 ||
                !_context.Categories.Any(c => c.CategoryId == p.CategoryId))
            {
                ModelState.AddModelError("", "Vui lòng chọn danh mục hợp lệ!");
                LoadCategoryDropdown();
                return View(p);
            }
        }

        // IMAGE 
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Path.GetFileName(imageFile.FileName);

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images",
                fileName
            );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            p.ImageUrl = fileName;
        }

        // SAVE 
        _context.Products.Add(p);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // EDIT

    // GET EDIT
    public IActionResult Edit(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        var product = _context.Products.Find(id);
        if (product == null)
            return NotFound();

        LoadCategoryDropdown();
        return View(product);
    }

    // POST EDIT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Product p, IFormFile imageFile, string NewCategory)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        var existing = _context.Products.Find(p.ProductId);
        if (existing == null)
            return NotFound();

        // CATEGORY 
        if (!string.IsNullOrWhiteSpace(NewCategory))
        {
            var cat = new Category { CategoryName = NewCategory };
            _context.Categories.Add(cat);
            _context.SaveChanges();

            existing.CategoryId = cat.CategoryId;
        }
        else
        {
            if (p.CategoryId == 0 ||
                !_context.Categories.Any(c => c.CategoryId == p.CategoryId))
            {
                ModelState.AddModelError("", "Danh mục không hợp lệ!");
                LoadCategoryDropdown();
                return View(p);
            }

            existing.CategoryId = p.CategoryId;
        }

        // UPDATE
        existing.ProductName = p.ProductName;
        existing.Price = p.Price;
        existing.Stock = p.Stock;
        existing.Description = p.Description;

        // IMAGE 
        if (imageFile != null && imageFile.Length > 0)
        {
            var fileName = Path.GetFileName(imageFile.FileName);

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images",
                fileName
            );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            existing.ImageUrl = fileName;
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // DELETE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        var sp = _context.Products.Find(id);

        if (sp != null)
        {
            _context.Products.Remove(sp);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    //EXPORT EXCEL
    public IActionResult ExportExcel()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        var products = _context.Products.ToList();

        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("SanPham");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Tên";
            ws.Cell(1, 3).Value = "Giá";
            ws.Cell(1, 4).Value = "Số lượng";
            ws.Cell(1, 5).Value = "Danh mục";

            int row = 2;

            foreach (var p in products)
            {
                ws.Cell(row, 1).Value = p.ProductId;
                ws.Cell(row, 2).Value = p.ProductName;
                ws.Cell(row, 3).Value = p.Price;
                ws.Cell(row, 4).Value = p.Stock;
                ws.Cell(row, 5).Value = p.CategoryId;
                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "SanPham.xlsx");
            }
        }
    }

    // THỐNG KÊ
    public IActionResult ThongKe()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Auth");

        ViewBag.Total = _context.Products.Count();
        ViewBag.Sum = _context.Products.Sum(x => x.Price * x.Stock);
        ViewBag.TotalOrder = _context.Orders.Count();
        ViewBag.TodayRevenue = _context.Orders
            .Where(x => x.OrderDate.Date == DateTime.Today)
            .Sum(x => (decimal?)x.TotalAmount) ?? 0;

        return View();
    }

    //LOAD DROPDOWN
    private void LoadCategoryDropdown()
    {
        ViewBag.Categories = new SelectList(
            _context.Categories.ToList(),
            "CategoryId",
            "CategoryName"
        );
    }
}