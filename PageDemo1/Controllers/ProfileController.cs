using Microsoft.AspNetCore.Mvc;
using PageDemo1.Models;
using System.Text.Json;

namespace PageDemo1.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ProfileController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Route("Profile/{id}")]
        public IActionResult Index(string id)
        {
            // โหลดไฟล์ JSON
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "posts.json");

            if (!System.IO.File.Exists(jsonPath))
            {
                return NotFound("ไม่พบข้อมูลโพสต์");
            }

            var jsonData = System.IO.File.ReadAllText(jsonPath);

            var posts = JsonSerializer.Deserialize<List<Post>>(jsonData);

            if (posts == null)
            {
                return NotFound("ข้อมูลโพสต์ไม่ถูกต้อง");
            }

            // กรองโพสต์ของ user ที่ส่งมา
            var userPosts = posts.Where(p => p.UserName == id).ToList();

            if (!userPosts.Any())
            {
                return NotFound($"ไม่พบโปรไฟล์ของ {id}");
            }

            // ใช้โพสต์แรกเป็นข้อมูลโปรไฟล์ (สมมุติ)
            var firstPost = userPosts.First();

            var profileViewModel = new ProfileViewModel
            {
                UserName = firstPost.UserName,
                ProfileImage = firstPost.ProfileImage,
                Bio = "สวัสดีครับ ผมชอบเล่นสล็อตแตกดีทุกวัน 🔥", // กำหนด bio เอง หรือจะดึงจาก JSON ถ้ามี
                Followers = 1500,
                Friends = 250,
                Posts = userPosts
            };

            return View(profileViewModel);
        }
    }
}
