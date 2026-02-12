namespace GeminiCode.Hunter.Domain.Core.Services;

public interface ICurlParserService
{
    (string Cookies, string XsrfToken, string DatasourceId, string ReportId, string SessionId) ParseCurlCommand(string curlCommand);
}
