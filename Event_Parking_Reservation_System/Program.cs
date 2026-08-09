using Elasticsearch.Net;
using Event_Parking_Reservation_System.Configuration;
using Event_Parking_Reservation_System.Data;
using Event_Parking_Reservation_System.Event_Parking_Reservation_System;
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Interfaces;
using Event_Parking_Reservation_System.Models;
using Event_Parking_Reservation_System.Repositories;
using Event_Parking_Reservation_System.Reposotires;
using Event_Parking_Reservation_System.Services;
using Event_Parking_Reservation_System_Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static Event_Parking_Reservation_System.Interfaces.IExternalgateways;
using static Event_Parking_Reservation_System.Interfaces.INotificationService;

namespace Event_Parking_Reservation_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("Booking"));

            builder.Services.AddDbContext<AppDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ---------- Services (DI) ----------
            builder.Services.AddScoped<ICustomerService, CustomerService>();

            // Services
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ISeatService, SeatService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IBookingNumberGenerator, BookingNumberGenerator>();

            // Missing dependencies
            builder.Services.AddScoped<Microsoft.AspNet.Identity.IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<SeatRepository>();
            IServiceCollection serviceCollection = builder.Services.AddScoped<Elasticsearch.Net.IDateTimeProvider, DateTimeProvider>();
            //builder.Services.AddScoped<IExternalgateways.IBookingGateway, BookingGateway>();



            builder.Services.AddScoped<IDashboardService, DashboardService>();
            // ---------- Repositories (DI) ----------
            // Identity password hasher
            builder.Services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();

            // Repositories
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<ISeatRepository, SeatRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

            // Extra dependencies



            //builder.Services.AddSingleton<IJwtService, JwtService>();
            //builder.Services.AddSingleton<IEmailService, EmailService>();

            // ---------- Controllers ----------
            builder.Services.AddControllers();

            // ---------- JWT Authentication ----------
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
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

            // ---------- CORS (for vanilla JS frontend served separately, e.g. Live Server) ----------
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5500",
                            "http://127.0.0.1:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ---------- Swagger ----------
            builder.Services.AddEndpointsApiExplorer();
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
                    Description = "Enter a valid JWT token (no 'Bearer ' prefix needed, Swagger adds it automatically)."
                };
                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() }
    });
            });

            // ---------- Background Services ----------


            var app = builder.Build();
            // ---------- Middleware pipeline ----------
            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("FrontendPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }


    }
}

    
