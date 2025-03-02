using BusinessLogic;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

            builder.Services.AddTransient<SheetMusicMakerDBContext>();

            builder.Services.AddTransient<IRepository<Recording>, RecordingRepository>();
            builder.Services.AddTransient<IRepository<Pdf>, PdfRepository>();

            builder.Services.AddTransient<IPdfLogic, PdfLogic>();
            builder.Services.AddTransient<IRecordingLogic, RecordingLogic>();

            var app = builder.Build();

            app.UseRouting();
            app.MapControllers();

            app.Run();
        }
    }
}
