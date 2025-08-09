using Microsoft.AspNetCore.Mvc;
using PageDemo1.Models;
using System.Text.Json;

namespace PageDemo1.Controllers
{
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private static bool isGenerating = false;
        private readonly OpenAIService _openAIService;

        public UserController(IWebHostEnvironment env, OpenAIService openAIService)
        {
            _env = env;
            _openAIService = openAIService;
        }

        [HttpGet]
        public IActionResult ManageUsers()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> StartGenerating([FromBody] GenerateRequest request)
        {
            if (isGenerating)
                return new JsonResult(new { status = "already-running" })
                {
                    ContentType = "application/json"
                };

            try
            {
                isGenerating = true;
                var newUsers = await GenerateUsers(request.count);
                isGenerating = false;

                return new JsonResult(new { status = "completed", users = newUsers })
                {
                    ContentType = "application/json"
                };
            }
            catch (Exception ex)
            {
                isGenerating = false;
                return new JsonResult(new { status = "error", message = ex.Message })
                {
                    ContentType = "application/json"
                };
            }
        }


        [HttpPost]
        public IActionResult StopGenerating()
        {
            isGenerating = false;
            return Json(new { status = "stopped" });
        }

        private async Task<List<User>> GenerateUsers(int count)
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "users.json");
            string jsonData = "[]";
            List<User> users = new List<User>();

            try
            {
                if (System.IO.File.Exists(jsonPath))
                {
                    jsonData = System.IO.File.ReadAllText(jsonPath);
                }

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    jsonData = "[]";
                }

                try
                {
                    users = JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Deserialize Error: " + ex.Message);
                    users = new List<User>();
                }

                int initialCount = users.Count;
                var newUsers = new List<User>();

                // เอาชื่อทั้งหมดไปเก็บใน HashSet เพื่อเช็คซ้ำ
                var existingUserNames = new HashSet<string>(users.Select(u => u.UserName));

                for (int i = 0; i < count && isGenerating; i++)
                {
                    string userName = string.Empty;

                    // Loop จนกว่าจะได้ชื่อที่ไม่ซ้ำ
                    int retryCount = 0;
                    do
                    {
                        var prompt = "ตอบกลับด้วย แค่ชื่อ เท่านั้น เช่น { น้องเมย์สายปั่น } อย่าตอบอย่างอื่น ให้ชื่อผู้ใช้สั้น ๆ น่ารัก ๆ แบบไทย ๆ ชื่อเดียวเท่านั้น";

                        var aiResponse = await _openAIService.GenerateTextAsync(prompt);

                        userName = aiResponse.Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                             .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))?
                                             .Trim() ?? $"AIUser_{initialCount + i + 1}";

                        retryCount++;

                        // กัน loop infinite (เช่น AI ตอบชื่อซ้ำเยอะมาก)
                        if (retryCount > 10)
                        {
                            userName = $"AIUser_{initialCount + i + 1}";
                            break;
                        }

                    } while (existingUserNames.Contains(userName));

                    existingUserNames.Add(userName);  // เพิ่มชื่อใหม่เข้า HashSet

                    var newUser = new User
                    {
                        UserName = userName,
                        ProfileImage = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
                        Bio = "" // Bio ว่าง
                    };

                    users.Add(newUser);
                    newUsers.Add(newUser);

                    await Task.Delay(1000); // จำลอง delay
                }

                // ตั้งค่าให้ไม่ escape unicode ภาษาไทย
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                jsonData = JsonSerializer.Serialize(users, options);
                System.IO.File.WriteAllText(jsonPath, jsonData);

                return newUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Error in GenerateUsers: " + ex.Message);
                return new List<User>();
            }
        }
        public class GenerateRequest
        {
            public int count { get; set; }
        }
    }
}
