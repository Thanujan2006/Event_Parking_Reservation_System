using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;
using Event_Parking_Reservation_System.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static Event_Parking_Reservation_System.Interfaces.IExternalgateways;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;
using static Event_Parking_Reservation_System.Services.IVenueCategoryRepository;
using IDateTimeProvider = Event_Parking_Reservation_System.Interfaces.IDateTimeProvider;

namespace Event_Parking_Reservation_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection(BookingSettings.SectionName));
            builder.Services.Configure<AuthTokenOptions>(builder.Configuration.GetSection("AuthTokens"));
            builder.Services.Configure<AdminSeedSettings>(builder.Configuration.GetSection(AdminSeedSettings.SectionName));

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ---------- Auth / security ----------
            builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            builder.Services.AddScoped<ICustomerAccountRepository, CustomerAccountRepositoryAdapter>();
            builder.Services.AddScoped<IAuthEmailSender, SmtpAuthEmailSender>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddSingleton<ITokenGenarater, TokenSecurity.CryptoTokenGenerator>();
            builder.Services.AddSingleton<ITokenHasher, TokenSecurity.Sha256TokenHasher>();

            // ---------- Application services ----------
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ISeatService, SeatService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IParkingService, ParkingService>();
            builder.Services.AddScoped<IBookingNumberGenerator, BookingNumberGenerator>();
            builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
            builder.Services.AddScoped<IReceiptGenerator, ReceiptGenerator>();
            builder.Services.AddScoped<IVenueService, VenueCategoryService.VenueService>();
            builder.Services.AddScoped<IEventCategoryService, VenueCategoryService.EventCategoryService>();
            builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();
            builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();

            // ---------- Payment gateways ----------
            builder.Services.AddScoped<IBookingGateway, BookingGateway>();
            builder.Services.AddScoped<ISeatPricingGateway, SeatPricingGateway>();
            builder.Services.AddScoped<IParkingFeeGateway, ParkingFeeGateway>();
            builder.Services.AddScoped<IPaymentNotificationGateway, PaymentNotificationGateway>();

            // ---------- Repositories ----------
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<SeatRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IVenueRepository, VenueCategoryRepository.VenueRepository>();
            builder.Services.AddScoped<IEventCategoryRepository, VenueCategoryRepository.EventCategoryRepository>();

            // ---------- Background jobs ----------
            builder.Services.AddHostedService<BookingExpiryBackgroundService>();

            // ---------- JWT Authentication ----------
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Jwt settings are missing from configuration.");

            if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
                throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });
            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["Frontend:BaseUrl"] ?? "https://localhost:7294",
                            "http://127.0.0.1:5500",
                            "http://localhost:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event & Parking Reservation System API", Version = "v1" });

                var jwtScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Enter a valid JWT token (Swagger adds the Bearer prefix)."
                };
                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // EnsureCreated matches the current EF model (owned customer security, venues, notifications).
                // Prefer `dotnet ef database update` once a clean migration set is generated.
                db.Database.EnsureCreated();
            }

            AdminSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();

            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseCors("FrontendPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
