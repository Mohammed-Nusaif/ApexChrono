using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//  debug code before building
var tempConfig = builder.Configuration;
var razorpayKey = tempConfig["Razorpay:Key"];
var razorpaySecret = tempConfig["Razorpay:Secret"];

Console.WriteLine("=== Program.cs Configuration Check ===");
Console.WriteLine($"Razorpay Key: {razorpayKey}");
Console.WriteLine($"Razorpay Secret: {razorpaySecret}");

if (string.IsNullOrEmpty(razorpayKey) || string.IsNullOrEmpty(razorpaySecret))
{
    Console.WriteLine("ERROR: Razorpay configuration is missing!");

    // List all configuration values for debugging
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
            "http://localhost:4200",      // Angular default dev server
            "https://localhost:4200",     // Angular with HTTPS
            "http://localhost:64307",     // Your provided URL
            "https://localhost:64307"     // Your provided URL with HTTPS
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

// SEED DATA CODE FOR PRODUCT
// ------------------------------------------------------
// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Check if we already have products (to avoid reseeding)
        if (!context.Products.Any())
        {
            var products = new List<Products>
            {
                new Products
                {
                    Name = "Apple Watch Series 9",
                    Price = 13999.00m,
                    Stock = 50,
                    Description = "Advanced smartwatch with Always-On Retina display, fitness tracking, and blood oxygen app",
                    Category = "Smart Watches",
                    Brand = "Apple",
                    Color = "Midnight",
                    ImageUrl = "https://images.unsplash.com/photo-1551816230-ef5deaed4a26?w=400",
                    DiscountPrice = 349.00m,
                    Rating = 4.5m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                },
                new Products
                {
                    Name = "Samsung Galaxy Watch 6",
                    Price = 7999.00m,
                    Stock = 75,
                    Description = "Premium smartwatch with advanced health monitoring, sleep coaching, and GPS tracking",
                    Category = "Smart Watches",
                    Brand = "Samsung",
                    Color = "Graphite",
                    ImageUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400",
                    DiscountPrice = 279.00m,
                    Rating = 4.3m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                },
                new Products
                {
                    Name = "Fitbit Versa 4",
                    Price = 2299.95m,
                    Stock = 100,
                    Description = "Health & fitness smartwatch with built-in GPS, Active Zone Minutes, and 6+ days battery",
                    Category = "Fitness Trackers",
                    Brand = "Fitbit",
                    Color = "Black",
                    ImageUrl = "https://images.unsplash.com/photo-1575311373937-040b8e1fd5b6?w=400",
                    DiscountPrice = 199.95m,
                    Rating = 4.2m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = false
                },
                new Products
                {
                    Name = "Garmin Venu 2",
                    Price = 3999.99m,
                    Stock = 40,
                    Description = "GPS smartwatch with AMOLED display, advanced health monitoring and fitness features",
                    Category = "Smart Watches",
                    Brand = "Garmin",
                    Color = "Slate",
                    ImageUrl = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=400",
                    Rating = 4.6m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                },
                new Products
                {
                    Name = "Fossil Gen 6",
                    Price = 2999.00m,
                    Stock = 60,
                    Description = "Smartwatch with Wear OS, heart rate tracking, GPS, and fast charging",
                    Category = "Smart Watches",
                    Brand = "Fossil",
                    Color = "Smoke",
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400",
                    DiscountPrice = 249.00m,
                    Rating = 4.1m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                },
                new Products
                {
                    Name = "Amazfit GTS 4",
                    Price = 1999.99m,
                    Stock = 85,
                    Description = "Ultra HD AMOLED display, 150+ sports modes, and 8-day battery life",
                    Category = "Smart Watches",
                    Brand = "Amazfit",
                    Color = "Infinite Black",
                    ImageUrl = "https://images.unsplash.com/photo-1434056886845-dac89ffe9b56?w=400",
                    Rating = 4.0m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                },
                new Products
                {
                    Name = "Withings ScanWatch",
                    Price = 2799.95m,
                    Stock = 30,
                    Description = "Hybrid smartwatch with medical-grade ECG and overnight oximetry",
                    Category = "Health Monitors",
                    Brand = "Withings",
                    Color = "Black",
                    ImageUrl = "https://images.unsplash.com/photo-1544117519-31a4b719223d?w=400",
                    Rating = 4.4m,
                    HasGPS = false,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = false
                },
                new Products
                {
                    Name = "Huawei Watch GT 3",
                    Price = 2299.99m,
                    Stock = 65,
                    Description = "2-week battery life, TruSeen 5.0+ heart rate monitoring, and 100+ workout modes",
                    Category = "Smart Watches",
                    Brand = "Huawei",
                    Color = "Active Black",
                    ImageUrl = "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=400",
                    DiscountPrice = 199.99m,
                    Rating = 4.2m,
                    HasGPS = true,
                    HasHeartRate = true,
                    HasSleepTracking = true,
                    HasBluetooth = true,
                    HasWaterResistance = true,
                    HasNFC = true
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Database seeded with 8 smart watch products!");
        }
        else
        {
            Console.WriteLine("ℹ️  Products already exist in database. Skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Seeding failed: {ex.Message}");
    }
}

// ------------------------------------------------------
// 8. Middleware Pipeline - ADD CORS MIDDLEWARE
// ------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();