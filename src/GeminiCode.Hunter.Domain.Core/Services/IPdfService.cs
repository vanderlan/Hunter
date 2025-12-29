using System.IO;
using System.Threading.Tasks;

namespace GeminiCode.Hunter.Domain.Core.Services;

public interface IPdfService
{
    Task<string> ExtractTextFromPdfAsync(Stream pdfStream);
}
