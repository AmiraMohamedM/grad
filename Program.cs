using grad.Data;
using grad.Models;
using grad.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Services -------------------
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<HomeService>();

// ------------------- JWT Authentication -------------------
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSection["Secret"];
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
});

// ------------------- Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Main API", Version = "v1" });
    c.SwaggerDoc("students", new OpenApiInfo { Title = "Student API", Version = "v1" });
    c.SwaggerDoc("teachers", new OpenApiInfo { Title = "Teacher API", Version = "v1" });
    c.SwaggerDoc("admin", new OpenApiInfo { Title = "Admin API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    c.AddSecurityDefinition(scheme.Reference.Id, scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });

    c.DocInclusionPredicate((doc, desc) =>
    {
        var path = desc.RelativePath?.ToLower() ?? "";
        if (doc == "students") return path.Contains("student");
        if (doc == "teachers") return path.Contains("teacher");
        if (doc == "admin") return path.Contains("admin");
        return doc == "v1";
    });
});

var app = builder.Build();

// ------------------- Middleware -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Main API");
        c.SwaggerEndpoint("/swagger/students/swagger.json", "Students");
        c.SwaggerEndpoint("/swagger/teachers/swagger.json", "Teachers");
        c.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin");
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===============================================================================
// STEP 1: Apply Database Migrations FIRST
// ===============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🔄 Applying database migrations...");

        var context = services.GetRequiredService<AppDbContext>();

        // Apply all pending migrations
        context.Database.Migrate();

        logger.LogInformation("✅ Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while migrating the database.");
        throw; // Stop the app if migrations fail
    }
}

// ===============================================================================
// STEP 2: Seed Roles & Admin User (AFTER migrations)
// ===============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🌱 Seeding roles and admin user...");

        // Seed roles
        await SeedData.SeedRolesAsync(services);
        await SeedData.SeedAdminAsync(services);

        // Seed Admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        string adminEmail = "m314227@gmail.com";
        string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                firstname = "Graduation",
                lastname = "Project",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("✅ Admin user created: {Email}", adminEmail);
        }
        else
        {
            logger.LogInformation("ℹ️ Admin user already exists: {Email}", adminEmail);
        }

        logger.LogInformation("✅ Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while seeding the database.");
        // Don't throw here - the app can still run without seed data
    }
}

// ===============================================================================
// STEP 3: Start the Application
// ===============================================================================
app.Run();