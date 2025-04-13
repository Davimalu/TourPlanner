using Microsoft.EntityFrameworkCore;
using TourPlanner.RestServer.DAL;
using TourPlanner.RestServer.DAL.Repository;
using TourPlanner.RestServer.DAL.Repository.Interfaces;
using TourPlanner.RestServer.Middleware;

namespace TourPlanner.RestServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            // Configure Dbcontext with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            // Register repositories
            builder.Services.AddScoped<ITourRepository, TourRepository>();
            builder.Services.AddScoped<ITourLogRepository, TourLogRepository>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            
            // Create a scope to obtain the AppDbContext instance and ensure the database is created.
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }
            
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
