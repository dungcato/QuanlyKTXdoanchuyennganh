using Microsoft.EntityFrameworkCore; // Thêm dòng này
using QuanlyKTX.Models;           // Thêm dòng này ?? nh?n di?n KTXContext

var builder = WebApplication.CreateBuilder(args);

// 1. L?y chu?i k?t n?i t? file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. ??ng ký KTXContext vào DI Container
builder.Services.AddDbContext<KTXContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cho phép s? d?ng các file t?nh trong wwwroot (CSS, JS, Hình ?nh)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();