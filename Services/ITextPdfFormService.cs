using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;

namespace ClaimsPortal.Services
{
    // Uses iText7 to fill AcroForm fields and flatten PDFs.
    public class ITextPdfFormService
    {
        public ITextPdfFormService()
        {
        }

        // Fill fields in an existing PDF template and flatten the form.
        // templatePath: absolute path to a PDF file that contains AcroForm fields
        // values: dictionary of fieldName -> value
        // outputPath: where to write the flattened PDF
        public Task FillAndFlattenAsync(string templatePath, IDictionary<string, string> values, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentNullException(nameof(templatePath));
            if (!File.Exists(templatePath)) throw new FileNotFoundException("PDF template not found", templatePath);

            var reader = new PdfReader(templatePath);
            var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(reader, writer);

            var form = PdfAcroForm.GetAcroForm(pdf, true);
            var fields = form.GetFormFields();

            foreach (var kv in values)
            {
                if (fields.ContainsKey(kv.Key))
                {
                    var f = fields[kv.Key];
                    f.SetValue(kv.Value ?? string.Empty);
                }
            }

            form.FlattenFields();
            pdf.Close();
            return Task.CompletedTask;
        }

        // Copy an editable PDF for preview/editing. If values provided, set them but do NOT flatten.
        public Task CopyPdfForEditingAsync(string templatePath, IDictionary<string, string>? values, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentNullException(nameof(templatePath));
            if (!File.Exists(templatePath)) throw new FileNotFoundException("PDF template not found", templatePath);

            // copy first, then optionally set default values (without flattening)
            File.Copy(templatePath, outputPath, overwrite:true);
            if (values == null || values.Count == 0) return Task.CompletedTask;

            var reader = new PdfReader(outputPath);
            // open with writer that writes back to same file
            var writer = new PdfWriter(outputPath + ".tmp");
            using var pdf = new PdfDocument(reader, writer);
            var form = PdfAcroForm.GetAcroForm(pdf, true);
            var fields = form.GetFormFields();
            foreach (var kv in values)
            {
                if (fields.ContainsKey(kv.Key)) fields[kv.Key].SetValue(kv.Value ?? string.Empty);
            }
            // do NOT flatten; just save
            pdf.Close();

            // replace original copy with written file
            File.Delete(outputPath);
            File.Move(outputPath + ".tmp", outputPath);
            return Task.CompletedTask;
        }

        // Extract form field values from a supplied filled PDF
        public Task<Dictionary<string, string>> ExtractFieldValuesAsync(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath)) throw new ArgumentNullException(nameof(pdfPath));
            if (!File.Exists(pdfPath)) throw new FileNotFoundException("PDF not found", pdfPath);

            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using var reader = new PdfReader(pdfPath);
            using var pdf = new PdfDocument(reader);
            var form = PdfAcroForm.GetAcroForm(pdf, false);
            if (form == null) return Task.FromResult(results);
            var fields = form.GetFormFields();
            foreach (var kv in fields)
            {
                var name = kv.Key;
                var field = kv.Value;
                var val = field.GetValueAsString();
                results[name] = val ?? string.Empty;
            }
            return Task.FromResult(results);
        }
    }
}
