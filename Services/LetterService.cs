using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Playwright;

namespace ClaimsPortal.Services
{
    public class LetterService
    {
        private readonly IWebHostEnvironment _env;

        public LetterService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string TemplatesPath => Path.Combine(_env.ContentRootPath, "wwwroot", "templates");

        public async Task<string> GeneratePdfFromTemplateAsync(string templateName, IDictionary<string, string> values, string outputFilePath)
        {
            var templateFile = Path.Combine(TemplatesPath, templateName);
            if (!File.Exists(templateFile))
                throw new FileNotFoundException("Template not found", templateFile);

            var html = await File.ReadAllTextAsync(templateFile, System.Text.Encoding.GetEncoding(1252));
            foreach (var kv in values)
            {
                var val = kv.Value ?? string.Empty;
                // Support multiple placeholder formats used in templates: {{Key}}, {{ Key }}, {Key}, { Key }
                html = html.Replace("{{" + kv.Key + "}}", val);
                html = html.Replace("{{ " + kv.Key + " }}", val);
                html = html.Replace("{" + kv.Key + "}", val);
                html = html.Replace("{ " + kv.Key + " }", val);
            }

            // Ensure directory
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? ".");

            // Use Playwright to render HTML and create PDF
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 992, Height = 1400 }
            });
            var page = await context.NewPageAsync();
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var pdfBytes = await page.PdfAsync(new PagePdfOptions { Format = "A4", PrintBackground = true });
            await File.WriteAllBytesAsync(outputFilePath, pdfBytes);
            return outputFilePath;
        }

        // Render raw HTML string to PDF using Playwright
        public async Task<string> GeneratePdfFromHtmlAsync(string html, string outputFilePath)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? ".");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 992, Height = 1400 }
            });
            var page = await context.NewPageAsync();
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var pdfBytes = await page.PdfAsync(new PagePdfOptions { Format = "A4", PrintBackground = true });
            await File.WriteAllBytesAsync(outputFilePath, pdfBytes);
            return outputFilePath;
        }
    }
}
