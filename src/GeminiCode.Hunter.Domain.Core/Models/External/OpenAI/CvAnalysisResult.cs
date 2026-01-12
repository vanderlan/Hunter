using System.Collections.Generic;

namespace GeminiCode.Hunter.Domain.Core.Models.External.OpenAI;

public record CvAnalysisResult(
    List<JobMatchResult> Matches
);

public record JobMatchResult(
    string JobCode,
    string JobTitle,
    decimal MatchPercentage,
    string MatchReason,
    List<string> MatchingSkills,
    List<string> MissingSkills,
    string ReferralReason,
    string WhatIsMissingForPerfectMatch
);
