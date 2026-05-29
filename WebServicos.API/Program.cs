using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebServicos.Data;
using WebServicos.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WebServicos API", Version = "v1" });
});

// CORS — permitir pedidos da aplicação Razor Pages
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazorApp", policy =>
        policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowRazorApp");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
