using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CẤU HÌNH SERILOG
// =========================================================================
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// =========================================================================
// 2. ĐĂNG KÝ DỊCH VỤ (TẤT CẢ builder.Services.Add... ĐẶT HẾT Ở ĐÂY)
// =========================================================================

// 2.1. Setup Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2.2. Setup Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 2.3. Cấu hình Identity Options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
});

// 2.4. Add Controllers & DbInitializer
builder.Services.AddControllers();
builder.Services.AddTransient<DbInitializer>();

// 2.5. Đăng ký Swagger (PHẢI ĐẶT TRƯỚC builder.Build())
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================================================================
// 3. BUILD APP & RUN SEEDING DATA
// =========================================================================
var app = builder.Build(); // <-- Sau dòng này, builder.Services bị KHÓA!

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Log.Information("Seeding data...");
        var dbInitializer = services.GetRequiredService<DbInitializer>();
        await dbInitializer.Seed();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// =========================================================================
// 4. CẤU HÌNH PIPELINE (Chỉ dùng các hàm app.Use...)
// =========================================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// Thêm UseAuthentication() trước UseAuthorization() để Identity xác thực token/cookie chuẩn xác
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();