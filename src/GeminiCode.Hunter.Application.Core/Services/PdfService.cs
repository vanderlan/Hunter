using System.Text;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using GeminiCode.Hunter.Domain.Core.Services;

namespace GeminiCode.Hunter.Application.Core.Services;

public class PdfService(ILogger<PdfService> logger) : IPdfService
{
    public async Task<string> ExtractTextFromPdfAsync(Stream pdfStream)
    {
        try
        {
            logger.LogInformation("Starting PDF text extraction");

            using var document = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                var text = page.Text;
                sb.AppendLine(text);
            }

            var extractedText = sb.ToString().Trim();
            
            logger.LogInformation("PDF text extraction completed. Extracted {Length} characters", extractedText.Length);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException("No text could be extracted from the PDF file");
            }

            return await Task.FromResult(extractedText);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from PDF");
            throw;
        }
    }
}
