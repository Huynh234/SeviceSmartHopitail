using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Services;
using SeviceSmartHopitail.Services.MAIL;
using SeviceSmartHopitail.Services.Profiles;
using SeviceSmartHopitail.Services.RAG;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình & Logging
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Logging.AddConsole();

// --- Kết nối DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<JWTServices>();

// --- Các service (điều chỉnh vòng đời DI)
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<TaiKhoanServices>();
builder.Services.AddScoped<MailServices>();
builder.Services.AddScoped<CloudinaryServices>();
builder.Services.AddScoped<UserProfileSevices>();
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<MangementAccountServices>();
builder.Services.AddScoped<QaServices>();

// Gemini SDK client wrapper: nên để Singleton nếu SDK sử dụng HttpClient/kết nối bên trong
builder.Services.AddSingleton<IGeminiClient, GeminiClientWrapper>();

// --- Cấu hình CORS (cho phép mọi nguồn gốc)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin();
            policyBuilder.AllowAnyHeader();
            policyBuilder.AllowAnyMethod();
        });
});

// --- Tiến trình chạy nền để tạo embeddings (chỉ cần nếu dữ liệu lớn)
//builder.Services.AddHostedService<EmbeddingBackgroundService>(); // cần tự implement class này nếu chưa có

// --- Scheduler gửi email tự động
builder.Services.AddHostedService<ScheduledEmailService>();

// --- Đăng ký PdfExtractor (nếu muốn gọi từ DI, không bắt buộc)
builder.Services.AddTransient<PdfExtractor>();

// --- Cấu hình Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SeviceSmartHopitail API", Version = "v1" });
});

// --- Cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "vu_dinh_huynh_vu_dinh_huynh_vu_dinh_huynh");

// --- cấu hình ssh
//builder.Services.AddHostedService<SshTunnelHostedService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// --- Tự động apply migrations khi khởi động (không khuyến nghị cho production)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- Chạy trong môi trường Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Gọi PdfExtractor 1 lần (nếu bạn đã chuẩn bị đường dẫn file PDF)
    //try
    //{
    //    var sp = app.Services;
    //    var extractor = sp.GetRequiredService<PdfExtractor>();
    //    // Đường dẫn file PDF — chỉnh lại theo môi trường của bạn
    //    PdfExtractor.Extract("D:/codes.txt", "D:/code_description_pairs.txt", "D:/descriptions.txt", app.Services);
    //}
    //catch (Exception ex)
    //{
    //    // Chỉ log lỗi, không dừng ứng dụng
    //    app.Logger.LogWarning(ex, "PdfExtractor lỗi (chỉ dev).");
    //}
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Services.AddAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
