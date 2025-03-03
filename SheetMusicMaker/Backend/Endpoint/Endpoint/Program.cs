using BusinessLogic;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using Repository;
using Repository.Generics;
using Repository.ModelRepos;

namespace Endpoint
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<SheetMusicMakerDBContext>();
            builder.Services.AddTransient<IRepository<Recording>, RecordingRepository>();
            builder.Services.AddTransient<IRepository<Pdf>, PdfRepository>();
            builder.Services.AddTransient<IPdfLogic, PdfLogic>();
            builder.Services.AddTransient<IRecordingLogic, RecordingLogic>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(x => x
                         .AllowCredentials()
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .WithOrigins("http://localhost:5173"));

            app.UseRouting();
            app.MapControllers();

            app.Run();
        }
    }
}
