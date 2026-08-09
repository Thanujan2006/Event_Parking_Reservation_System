
using Event_Parking_Reservation_System.Exceptions;
using Event_Parking_Reservation_System.Repositories;

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

            //builder.Services.AddScoped<ISeatRepository, SeatRepository>();

            
            //.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RegisterCustomerRequestValidator>());


            var app = builder.Build();
            app.UseMiddleware<CustomerExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
