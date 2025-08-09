using Microsoft.AspNetCore.Mvc;
using PageDemo1.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace PageDemo1.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly OpenAIService _openAIService;

        public AdminController(IWebHostEnvironment env, OpenAIService openAIService)
        {
            _env = env;
            _openAIService = openAIService;
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "users.json");
            List<string> userNames = new List<string>();

            if (System.IO.File.Exists(jsonPath))
            {
                var jsonData = System.IO.File.ReadAllText(jsonPath);

                try
                {
                    var users = JsonSerializer.Deserialize<List<User>>(jsonData);
                    if (users != null)
                    {
                        userNames = users.Select(u => u.UserName).ToList();
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Error reading users.json: " + ex.Message);
                }
            }

            var model = new CreatePostViewModel
            {
                UserNames = userNames
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult SavePost(CreatePostViewModel model)
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "posts.json");
            var jsonData = System.IO.File.ReadAllText(jsonPath);
            var posts = JsonSerializer.Deserialize<List<Post>>(jsonData) ?? new List<Post>();

            // ดึงค่า ImageUrl จาก Form
            var imageUrl = Request.Form["ImageUrl"];

            posts.Insert(0, new Post
            {
                UserName = model.SelectedUser,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Content = model.Content,
                ProfileImage = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
                ImageUrl = imageUrl
            });

            jsonData = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(jsonPath, jsonData);

            return RedirectToAction("Index", "Home");
        }

        public class PromptRequest
        {
            public string Prompt { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateContent([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Json(new { content = "กรุณาใส่ข้อความสำหรับสร้างเนื้อหา" });
            }

            try
            {
                var generatedContent = await _openAIService.GenerateTextAsync(request.Prompt);
                return Json(new { content = generatedContent });
            }
            catch (Exception ex)
            {
                return Json(new { content = $"เกิดข้อผิดพลาด: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult GetImageFolders()
        {
            var imgRootPath = Path.Combine(_env.WebRootPath, "img");
            var folderNames = new List<string>();

            if (Directory.Exists(imgRootPath))
            {
                folderNames = Directory.GetDirectories(imgRootPath)
                                       .Select(Path.GetFileName)
                                       .ToList();
            }

            return Json(folderNames);
        }

        [HttpGet]
        public IActionResult GetRandomImage(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return Json(new { imageUrl = "" });

            var folderPath = Path.Combine(_env.WebRootPath, "img", folderName);

            if (!Directory.Exists(folderPath))
                return Json(new { imageUrl = "" });

            var imageFiles = Directory.GetFiles(folderPath, "*.*")
                                      .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                  f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                                  f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                                      .ToList();

            if (imageFiles.Count == 0)
                return Json(new { imageUrl = "" });

            var random = new Random();
            var randomImage = Path.GetFileName(imageFiles[random.Next(imageFiles.Count)]);

            var imageUrl = $"/img/{folderName}/{randomImage}";

            return Json(new { imageUrl });
        }

    }
}
