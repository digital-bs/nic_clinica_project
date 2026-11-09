using NIC_clinica_project.Models;
using NIC_clinica_project.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NIC_clinica_project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<ICrudRepository<ClientsDtoWithKeys, ClientsDtoWithValues>, ClientsRepository>();
            builder.Services.AddScoped<ICrudRepository<DoctorsDtoWithKeys, DoctorsDtoWithValues>, DoctorsRepository>();

            var connectionString = builder.Configuration.GetConnectionString("ClinicaDb");
            builder.Services.AddDbContext<ClinicaDbContext>(options => options.UseSqlServer(connectionString));


            builder.Services.AddControllers();  
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
