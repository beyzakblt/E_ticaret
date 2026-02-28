using E_ticaret.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace E_ticaret.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Veritabanı bağlantısı için context ekledik
        private readonly EticaretContext _context;

        // Constructor'a _context'i dahil ettik
        public HomeController(ILogger<HomeController> logger, EticaretContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Veritabanındaki ürünleri listeliyoruz
            var urunler = _context.Urunlers.ToList();
            return View(urunler);
        }

        public IActionResult Kullanicilar()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}