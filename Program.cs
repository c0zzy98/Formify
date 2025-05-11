using Formify.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dodanie Razor Pages
builder.Services.AddRazorPages();

// Dodanie sesji
builder.Services.AddSession();

// Dodanie EF Core i DB Context (twój kod)
builder.Services.AddDbContext<FormifyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Użycie sesji
app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// 🔽 TO DODAJ NA KOŃCU:
app.MapFallbackToPage("/Login");

app.Run();
