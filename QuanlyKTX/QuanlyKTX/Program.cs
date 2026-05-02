using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies; // Thư viện Bắt buộc cho Đăng nhập
using QuanlyKTX.Models;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CẤU HÌNH DATABASE & CẦU NỐI CONTEXT
// =========================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký Context gốc (file của bạn ông tạo ra)
builder.Services.AddDbContext<KTXContext>(options =>
    options.UseSqlServer(connectionString));

// KÍCH HOẠT CẦU NỐI: Đăng ký KtxthongminhContext để các Controller cũ vẫn chạy bình thường
builder.Services.AddScoped<KtxthongminhContext>();


// =========================================================
// 2. CẤU HÌNH BẢO MẬT & PHÂN QUYỀN (AUTHENTICATION)
// =========================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Khi người dùng chưa đăng nhập mà ấn vào trang quản trị, hệ thống tự động đá về đây
        options.LoginPath = "/Admin/TaiKhoan/Login";

        // Khi nhân viên (không phải Admin) cố tình vào trang của Admin, sẽ bị đá về đây
        options.AccessDeniedPath = "/Admin/TaiKhoan/AccessDenied";

        options.Cookie.Name = "KtxAuthCookie";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tự động đăng xuất sau 8 tiếng
        options.SlidingExpiration = true;
    });


// =========================================================
// 3. CẤU HÌNH SESSION & MVC
// =========================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();


// =========================================================
// 4. THIẾT LẬP MIDDLEWARE PIPELINE
// =========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cho phép sử dụng các file tĩnh trong wwwroot (CSS, JS, Hình ảnh)
app.UseStaticFiles();

app.UseRouting();

// Bật Session
app.UseSession();

// HAI DÒNG NÀY LÀ CỐT LÕI BẢO MẬT (Lưu ý: Authentication luôn phải nằm trước Authorization)
app.UseAuthentication(); // Xác thực xem "Bạn là ai?"
app.UseAuthorization();  // Xác thực xem "Bạn có quyền làm việc này không?"

// Cấu hình Đường dẫn (Routing)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();