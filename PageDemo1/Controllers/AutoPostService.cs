using Microsoft.Extensions.Hosting;
using PageDemo1.Controllers;
using PageDemo1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class AutoPostService : BackgroundService
{
    private readonly IWebHostEnvironment _env;
    private readonly OpenAIService _openAIService;
    private readonly Random _random = new();

    private const string FixedMessage = "ช่วยสร้างโพส ข้อความประมาณนี้แต่ขอแบบมีความเป็นคนที่เพะิงได้เงินมามากๆ เพื่อนๆ ครับ! พึ่งลองเล่นเว็บนี้มาล่าสุด แตกดีมากจริงๆ! ยิ่งเล่นยิ่งสนุก โบนัสก็มาแบบรัวๆ ไม่อยากให้พลาดเลย รีบมาลองกันนะ! #เว็บสล็อตแตกดี";

    public AutoPostService(IWebHostEnvironment env, OpenAIService openAIService)
    {
        _env = env;
        _openAIService = openAIService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CreateRandomPostAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutoPostService] Error: {ex.Message}");
            }

            //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // 2 นาทีตามที่ต้องการ
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        }
    }

    private async Task CreateRandomPostAsync()
    {
        // โหลดรายชื่อผู้ใช้
        var userJsonPath = Path.Combine(_env.WebRootPath, "data", "users.json");
        if (!File.Exists(userJsonPath))
            return;

        var usersJson = File.ReadAllText(userJsonPath);
        var users = JsonSerializer.Deserialize<List<User>>(usersJson) ?? new List<User>();
        if (users.Count == 0)
            return;

        var randomUser = users[_random.Next(users.Count)].UserName;

        // เลือกโฟลเดอร์รูปใน img
        var imgRoot = Path.Combine(_env.WebRootPath, "img");
        if (!Directory.Exists(imgRoot))
            return;

        var folders = Directory.GetDirectories(imgRoot);
        if (folders.Length == 0)
            return;

        var randomFolder = folders[_random.Next(folders.Length)];
        var folderName = Path.GetFileName(randomFolder);

        // เลือกรูปในโฟลเดอร์นั้น
        var images = Directory.GetFiles(randomFolder)
            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (images.Count == 0)
            return;

        var randomImage = Path.GetFileName(images[_random.Next(images.Count)]);
        var imageUrl = $"/img/{folderName}/{randomImage}";

        // เรียก OpenAI สร้างข้อความจาก prompt (FixedMessage)
        var generatedContent = await _openAIService.GenerateTextAsync(FixedMessage);

        // โหลดโพสต์เก่า
        var postJsonPath = Path.Combine(_env.WebRootPath, "data", "posts.json");
        var posts = new List<Post>();
        if (File.Exists(postJsonPath))
        {
            var postsJson = File.ReadAllText(postJsonPath);
            posts = JsonSerializer.Deserialize<List<Post>>(postsJson) ?? new List<Post>();
        }

        // สร้างโพสต์ใหม่
        // สร้างโพสต์ใหม่
        posts.Insert(0, new Post
        {
            UserName = randomUser,
            Time = DateTime.Now.ToString("dd MMM yyyy HH:mm", new System.Globalization.CultureInfo("th-TH")),
            Content = generatedContent,
            ProfileImage = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            ImageUrl = imageUrl,

            Likes = _random.Next(1500, 5001),  // สุ่ม 1500 ถึง 3000
            Shares = _random.Next(1500, 5001),
            Saves = _random.Next(1500, 5001)
        });


        var updatedJson = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(postJsonPath, updatedJson);

        // Console.WriteLine($"[AutoPostService] สร้างโพสต์ใหม่โดย {randomUser} รูปจากโฟลเดอร์ {folderName}");
    }
}
