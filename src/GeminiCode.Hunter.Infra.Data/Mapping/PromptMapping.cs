using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Constants;

namespace GeminiCode.Hunter.Infra.Data.Mapping;

public class PromptMapping : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.Property(x => x.PublicId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Text).HasColumnType("text").IsRequired();
        
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.PublicId).IsUnique();

        // Configure DateTime properties to use 'timestamp without time zone'
        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .HasColumnType("timestamp without time zone")
            .IsRequired();

        // Seed default prompts
        builder.HasData(
            new Prompt
            {
                PromptId = 1,
                PublicId = Guid.NewGuid().ToString(),
                Key = PromptConstants.JobEnrichment,
                Name = "Job Enrichment Prompt",
                Text = GetJobEnrichmentPromptTemplate(),
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            },
            new Prompt
            {
                PromptId = 2,
                PublicId = Guid.NewGuid().ToString(),
                Key = PromptConstants.CvAnalysis,
                Name = "CV Analysis Prompt",
                Text = GetCvAnalysisPromptTemplate(),
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            }
        );
    }

    private static string GetJobEnrichmentPromptTemplate()
    {
        return @"# TASK
Extract structured information from the following job posting for: **{jobTitle}**

# JOB POSTING HTML CONTENT
```html
{rawHtmlContent}
```

# EXTRACTION INSTRUCTIONS

Extract and structure the following information:

1. **Summary**: A concise 2-3 sentence overview of the position
2. **Responsibilities**: Key duties and responsibilities (bullet points or paragraphs)
3. **Requirements**: ALL position requirements including education, experience level, certifications, and general qualifications
   - Example: ""Bachelor's degree in Computer Science, 5+ years of experience, AWS certification, Strong problem-solving skills""
4. **Must-Have Skills**: Essential/required technical and soft skills only, comma-separated
5. **Nice-to-Have Skills**: Preferred/optional skills, comma-separated

# IMPORTANT RULES
- Extract ONLY information present in the HTML content
- Do NOT invent or assume information
- If a section has no data, return empty string
- For Requirements: include education, experience level, years, certifications, and general qualifications
- Format requirements as comma-separated values or structured text
- Keep text concise and professional

# OUTPUT FORMAT
Return a JSON object with this exact structure:
```json
{
  ""summary"": ""string"",
  ""responsibilities"": ""string"",
  ""requirements"": ""string"",
  ""mustHaveSkills"": ""string"",
  ""niceToHaveSkills"": ""string""
}
```";
    }

    private static string GetCvAnalysisPromptTemplate()
    {
        return @"# TASK
Analyze the following CV against the provided job positions and determine the best matches.

# CANDIDATE CV
```
{cvText}
```

# AVAILABLE JOB POSITIONS

{jobPositions}

# INSTRUCTIONS
For each job position:
1. Calculate a match percentage (0-100) based on:
   - Skills alignment (technical and soft skills)
   - Experience level and years of experience
   - Education background
   - Requirements compliance (certifications, qualifications)
   - Responsibilities match
2. Provide a clear, concise reason for the match percentage
3. List specific matching skills found in the CV
4. List skills required by the job but missing from the CV
5. Generate 'What is missing for a perfect match?' (2-4 sentences) explaining:
   - Specific skills, experiences, or qualifications the candidate needs to develop
   - Areas where the candidate could improve to reach 100% match
   - Be constructive and actionable

# CRITICAL EVALUATION CRITERIA
Be extremely critical about the following requirements and SEVERELY penalize candidates who do not meet them:

1. **Years of Experience:**
   - Carefully analyze the candidate's total years of professional experience
   - Compare against the required years specified in the Requirements section
   - If candidate has LESS years than required, apply severe penalty (reduce match by 20-40%)
   - Example: Position requires 5+ years, candidate has 2 years → Maximum match should be 50-60%
   - Exception: Only if candidate shows exceptional skills that clearly compensate, allow up to 70% match

2. **Primary Technology Stack:**
   - If position requires full-stack development but candidate only has front-end OR back-end experience → Severely penalize
   - If candidate specializes in one technology but position requires a different core technology (e.g., Java vs C#) → Severely penalize
   - Example: Position requires C# expertise, candidate is Java specialist → Maximum match 60-65% even with many years of experience

3. **Education & Certifications:**
   - If Requirements specify degree level (Bachelor's, Master's) and candidate doesn't have it → Apply penalty (reduce match by 10-15%)
   - If Requirements specify certifications (AWS, Azure, etc.) and candidate lacks them → Apply penalty (reduce match by 5-10%)

Return ONLY positions with match percentage > {minimumMatchPercentage}%, ordered by match percentage (highest first).
Validate CV File if you are not able to find important information such as: Name, e-mail, phone number or the file is inconsistent with a usual CV format, reject then and returns no matches.

# OUTPUT FORMAT
Return a JSON object with this exact structure:
```json
{
  ""matches"": [
    {
      ""jobCode"": ""string"",
      ""jobTitle"": ""string"",
      ""matchPercentage"": 85.5,
      ""matchReason"": ""string"",
      ""matchingSkills"": [""skill1"", ""skill2""],
      ""missingSkills"": [""skill3"", ""skill4""],
      ""whatIsMissingForPerfectMatch"": ""string""
    }
  ]
}
```";
    }
}
