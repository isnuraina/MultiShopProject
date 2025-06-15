using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShopProject.Context;
using MultiShopProject.ViewModels.Home;

namespace MultiShopProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext context;
        public HomeController(DataContext _context)
        {
            context = _context;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM
            {
                CarouselItems= await context.CarouselItems.ToListAsync(),
                CardItems = await context.CardItems.ToListAsync(),
                Sponsors=await context.Sponsors.ToListAsync(),
                Categories = await context.Categories
                              .Include(m => m.Products)
                              .ToListAsync(),

                Products = await context.Products
                            .Include(m => m.Category)
                            .ToListAsync()
            };
            return View(homeVM);
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
    }
}
