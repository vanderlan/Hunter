namespace GeminiCode.Hunter.Domain.Core.Constants;

public static class DomainConstants
{
    public static class AIProviders
    {
        public const string OpenAI = "OpenAI";
        public const string DeepSeek = "DeepSeek";
        public const string DeepSeekUrl = "https://api.deepseek.com";
    }

    public static class AIMatching
    {
        public const int MinimumMatchPercentage = 70;
        public const int HighMatchPercentage = 70;
        public const int MediumMatchPercentage = 50;
    }
}