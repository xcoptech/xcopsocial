using DotNetEnv;
using PageDemo1.Controllers;

Env.Load();  // โหลด .env ก่อนสร้าง builder

var builder = WebApplication.CreateBuilder(args);

// เพิ่มบริการ MVC
builder.Services.AddControllersWithViews();

// ลงทะเบียน OpenAIService ใน DI container
builder.Services.AddSingleton<OpenAIService>(sp => {
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    return new OpenAIService(apiKey);
});

// ลงทะเบียน BackgroundService สำหรับ AutoPost
builder.Services.AddHostedService<AutoPostService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// กำหนด route เริ่มต้น
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
