using AnalyzerService;
using BusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository;
using System;
using System.IO;

namespace Endpoint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            ResetDirectories(builder);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<SheetMusicMakerDBContext>();
            builder.Services.AddTransient<IFileRepository, FileRepository>();
            builder.Services.AddTransient<IBusinessLogic, BusinessLogic.BusinessLogic>();
            builder.Services.AddTransient<IAudioAnalyzer, AudioAnalyzer>();

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
                         .WithOrigins(["http://localhost:5173", "https://dev-levy.github.io"]));

            app.UseRouting();
            app.MapControllers();

            app.MapGet("/health", () => Results.Ok());

            app.Run();
        }

        private static void ResetDirectories(WebApplicationBuilder builder)
        {
            string uploadPath = builder.Configuration["FileStorage:UploadDir"] ?? throw new ArgumentException("Config is faulty! UploadDir not found!");
            string createPath = builder.Configuration["FileStorage:CreatedDir"] ?? throw new ArgumentException("Config is faulty! CreatedDir not found!");

            if (Directory.Exists(uploadPath))
                Directory.Delete(uploadPath, recursive: true);
            Directory.CreateDirectory(uploadPath);

            if (Directory.Exists(createPath))
                Directory.Delete(createPath, recursive: true);
            Directory.CreateDirectory(createPath);
        }
    }
}
