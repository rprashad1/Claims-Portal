using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ClaimsPortal.Services
{
    // Lightweight service for working with templates and form-data.
    // NOTE: This implementation uses the existing HTML template + Playwright renderer
    // for preview/final generation. It exposes helpers to extract placeholders
    // and to generate a preview/final PDF by substituting values.
    public class PdfFormService
    {
        private readonly IWebHostEnvironment _env;
        private readonly LetterService _letterService;

        public PdfFormService(IWebHostEnvironment env, LetterService letterService)
        {
            _env = env;
            _letterService = letterService;
        }

        public Task<string[]> ExtractPlaceholdersFromTemplateAsync(string templateName)
        {
            var templatesDir = Path.Combine(_env.ContentRootPath, "wwwroot", "templates");
            var path = Path.Combine(templatesDir, templateName);
            if (!File.Exists(path)) throw new FileNotFoundException("Template not found", path);

            var html = File.ReadAllText(path);
            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in Regex.Matches(html, "\\{\\{\\s*(.*?)\\s*\\}\\}|\\{\\s*(.*?)\\s*\\}"))
            {
                var g = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(g)) fields.Add(g.Trim());
            }

            return Task.FromResult(fields.ToArray());
        }

        public Task<string> GeneratePreviewPdfAsync(string templateName, IDictionary<string, string> values, string outputFilePath)
        {
            // Leverage LetterService which uses Playwright to render HTML->PDF.
            return _letterService.GeneratePdfFromTemplateAsync(templateName, values, outputFilePath);
        }

        public Task<string> FillAndFlattenAsync(string templateName, IDictionary<string, string> values, string outputFilePath)
        {
            // For now, identical to GeneratePreviewPdfAsync since Playwright renders
            // a final PDF with placeholders replaced. In future this can apply
            // true AcroForm field population + flattening using a PDF library.
            return _letterService.GeneratePdfFromTemplateAsync(templateName, values, outputFilePath);
        }
    }
}

