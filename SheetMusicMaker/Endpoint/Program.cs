using BusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository;

namespace Endpoint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<SheetMusicMakerDBContext>();
            builder.Services.AddTransient<IFileRepository, FileRepository>();
            builder.Services.AddTransient<IBusinessLogic, BusinessLogic.BusinessLogic>();

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(x => x
                         .AllowCredentials()
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .WithExposedHeaders("Content-Disposition")
                         .WithOrigins("http://localhost:5173"));

            app.UseRouting();
            app.MapControllers();

            app.Run();

        }
    }
}
