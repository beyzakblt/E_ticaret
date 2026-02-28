using E_ticaret.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (Context Kaydı)
builder.Services.AddDbContext<EticaretContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

// 2. Session (Oturum) ve Cache Servisleri
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Güvenlik için eklendi
    options.Cookie.IsEssential = true; // KVKK/GDPR için eklendi
});

// 3. MVC Controller ve View Desteği
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- HTTP Request Pipeline (Middleware) Ayarları ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ÖNEMLİ: UseSession, UseRouting'den SONRA, UseAuthorization'dan ÖNCE gelmelidir.
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();