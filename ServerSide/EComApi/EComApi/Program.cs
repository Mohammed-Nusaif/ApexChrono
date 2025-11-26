using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Razorpay Debug Logging
// ------------------------------------------------------
var tempConfig = builder.Configuration;
var razorpayKey = tempConfig["Razorpay:Key"];
var razorpaySecret = tempConfig["Razorpay:Secret"];

Console.WriteLine("=== Program.cs Configuration Check ===");
Console.WriteLine($"Razorpay Key: {razorpayKey}");
Console.WriteLine($"Razorpay Secret: {razorpaySecret}");

if (string.IsNullOrEmpty(razorpayKey) || string.IsNullOrEmpty(razorpaySecret))
{
    Console.WriteLine("ERROR: Razorpay configuration is missing!");

    foreach (var config in tempConfig.AsEnumerable())
    {
        if (config.Key.Contains("Razorpay", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Config: {config.Key} = {config.Value}");
        }
    }
}

// ------------------------------------------------------
// 1. CORS Configuration 
// ------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "http://localhost:64307",
            "https://localhost:64307"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ------------------------------------------------------
// 2. Database Configuration
// ------------------------------------------------------
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ------------------------------------------------------
// 3. Identity Configuration
// ------------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ------------------------------------------------------
// 4. JWT Authentication Configuration
// ------------------------------------------------------
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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role
    };
});

// ------------------------------------------------------
// 5. Dependency Injection (Services)
// ------------------------------------------------------
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// ------------------------------------------------------
// 6. Controllers
// ------------------------------------------------------
builder.Services.AddControllers();

// ------------------------------------------------------
// 7. Swagger Configuration (with JWT support)
// ------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECom API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ------------------------------------------------------
// Build Application
// ------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------
// SEED DATA (Optional)
// ------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!context.Products.Any())
        {
            var products = new List<Products>
            {
                new Products
                {
                    Name = "Samsung Galaxy Watch 6",
                    Brand = "Samsung",
                    Category = "Smart Watches",
                    Description = "Premium smartwatch with advanced health tracking, sleep analysis, and AMOLED display.",
                    BasePrice = 34999.00m,
                    DiscountPrice = 29999.00m,
                    Rating = 4.5m,
                    ThumbnailUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400",
                    ImageUrls = new List<string>
                    {
                        "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400",
                        "https://images.unsplash.com/photo-1623479322729-28b26dfdc1d8?w=400",
                        "https://images.unsplash.com/photo-1621418105239-c512b9d20a23?w=400"
                    },
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant { Color = "Graphite", Price = 34999.00m, Stock = 40 },
                        new ProductVariant { Color = "Silver", Price = 34999.00m, Stock = 25 },
                        new ProductVariant { Color = "Rose Gold", Price = 35999.00m, Stock = 15 }
                    },
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Database seeded with products");
        }
        else
        {
            Console.WriteLine("ℹ️ Products already exist. Skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
    }
}

// ------------------------------------------------------
// 8. Middleware Pipeline
// ------------------------------------------------------

app.UseHttpsRedirection();

// ⭐ FIX IMAGE PROBLEM: Serve static files from wwwroot
app.UseStaticFiles();
// Serve /uploads/products from backend
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products")
    ),
    RequestPath = "/uploads/products"
});


app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECom API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();
