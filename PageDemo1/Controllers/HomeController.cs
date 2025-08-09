using Microsoft.AspNetCore.Mvc;
using PageDemo1.Models;
using System.Diagnostics;
using System.Text.Json;

namespace PageDemo1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;

        }

        public IActionResult Index()
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "posts.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var posts = JsonSerializer.Deserialize<List<Post>>(jsonData);

            return View(posts);
        }
        public IActionResult Privacy()
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
