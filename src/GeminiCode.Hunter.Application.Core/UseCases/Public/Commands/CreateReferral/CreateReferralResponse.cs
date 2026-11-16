namespace GeminiCode.Hunter.Application.Core.UseCases.Public.Commands.CreateReferral;

public class CreateReferralResponse
{
    public string JobTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    /// <summary>True when the request will only reach internal users after the candidate confirms their email.</summary>
    public bool IsEmailConfirmationRequired { get; set; }
}
