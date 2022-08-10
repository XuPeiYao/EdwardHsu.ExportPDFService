using System.CodeDom;
using System.CodeDom.Compiler;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace EdwardHsu.ExportPDFService
{
    public class Program
    {
        private static Browser _chromeBrowser = default;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton(sp =>
            {
                return _chromeBrowser = Puppeteer.LaunchAsync(new LaunchOptions
                {
                    ExecutablePath = "/usr/bin/chromium",
                    Headless = true,
                    Args = new string[] { "--no-sandbox" }
                }).GetAwaiter().GetResult();
            });

            
            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapPost("/api/export", async (
                [FromBody] ExportRequest request,
                [FromServices] Browser chrome
                ) =>
            {
                using var tab = await _chromeBrowser.NewPageAsync();

                var escapeHtml = ToLiteral(request.html);
                var result = await tab.EvaluateExpressionAsync("document.write(" + escapeHtml + ");");

                var tmpFile = Path.GetTempFileName();

                var size = PaperFormat.A4;

                try
                {
                    size = (PaperFormat)typeof(PaperFormat).GetProperty(request.PaperSize.ToString(),
                        BindingFlags.Static | BindingFlags.Public).GetValue(null);
                }catch{}

                await tab.PdfAsync(tmpFile, new PdfOptions()
                {
                    DisplayHeaderFooter = false,
                    Format = size
                });
                await tab.CloseAsync();

                var pdfStream = new FileStream(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4*1025, FileOptions.DeleteOnClose);

                return Results.File(pdfStream, "application/pdf", $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.pdf");
            });
            
            app.Run();

            if (_chromeBrowser != null)
            {
                _chromeBrowser.CloseAsync().GetAwaiter().GetResult();
                _chromeBrowser.Dispose();
            }
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
    }
}