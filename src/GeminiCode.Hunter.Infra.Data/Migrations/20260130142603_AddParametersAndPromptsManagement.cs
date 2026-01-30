using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GeminiCode.Hunter.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParametersAndPromptsManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parameter",
                columns: table => new
                {
                    ParameterId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parameter", x => x.ParameterId);
                });

            migrationBuilder.CreateTable(
                name: "Prompt",
                columns: table => new
                {
                    PromptId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompt", x => x.PromptId);
                });

            migrationBuilder.InsertData(
                table: "Parameter",
                columns: new[] { "ParameterId", "CreatedAt", "Key", "LastUpdated", "PublicId", "Value" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ACTIVE_AI_SERVICE", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "7dc4789d-5000-41c3-b2b6-df235e61a1f1", "DeepSeek" },
                    { 2L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MINIMUM_MATCH_PERCENTAGE_CV_ANALYSIS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "68fc56f8-9de9-42f0-9e61-b5dc802c6097", "70" },
                    { 3L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MINIMUM_MATCH_PERCENTAGE_AUTO_MATCHING", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "471c1dc2-1032-4f50-96f0-03ad780c3129", "70" }
                });

            migrationBuilder.InsertData(
                table: "Prompt",
                columns: new[] { "PromptId", "CreatedAt", "Key", "LastUpdated", "Name", "PublicId", "Text" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "JOB_ENRICHMENT", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Job Enrichment Prompt", "955c3725-c1e0-49f0-8a39-c5dc86717a59", "# TASK\nExtract structured information from the following job posting for: **{jobTitle}**\n\n# JOB POSTING HTML CONTENT\n```html\n{rawHtmlContent}\n```\n\n# EXTRACTION INSTRUCTIONS\n\nExtract and structure the following information:\n\n1. **Summary**: A concise 2-3 sentence overview of the position\n2. **Responsibilities**: Key duties and responsibilities (bullet points or paragraphs)\n3. **Requirements**: ALL position requirements including education, experience level, certifications, and general qualifications\n   - Example: \"Bachelor's degree in Computer Science, 5+ years of experience, AWS certification, Strong problem-solving skills\"\n4. **Must-Have Skills**: Essential/required technical and soft skills only, comma-separated\n5. **Nice-to-Have Skills**: Preferred/optional skills, comma-separated\n\n# IMPORTANT RULES\n- Extract ONLY information present in the HTML content\n- Do NOT invent or assume information\n- If a section has no data, return empty string\n- For Requirements: include education, experience level, years, certifications, and general qualifications\n- Format requirements as comma-separated values or structured text\n- Keep text concise and professional\n\n# OUTPUT FORMAT\nReturn a JSON object with this exact structure:\n```json\n{\n  \"summary\": \"string\",\n  \"responsibilities\": \"string\",\n  \"requirements\": \"string\",\n  \"mustHaveSkills\": \"string\",\n  \"niceToHaveSkills\": \"string\"\n}\n```" },
                    { 2L, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CV_ANALYSIS", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CV Analysis Prompt", "20b36e63-1259-42b5-a068-9ffa12b9c14f", "# TASK\nAnalyze the following CV against the provided job positions and determine the best matches.\n\n# CANDIDATE CV\n```\n{cvText}\n```\n\n# AVAILABLE JOB POSITIONS\n\n{jobPositions}\n\n# INSTRUCTIONS\nFor each job position:\n1. Calculate a match percentage (0-100) based on:\n   - Skills alignment (technical and soft skills)\n   - Experience level and years of experience\n   - Education background\n   - Requirements compliance (certifications, qualifications)\n   - Responsibilities match\n2. Provide a clear, concise reason for the match percentage\n3. List specific matching skills found in the CV\n4. List skills required by the job but missing from the CV\n5. Generate 'What is missing for a perfect match?' (2-4 sentences) explaining:\n   - Specific skills, experiences, or qualifications the candidate needs to develop\n   - Areas where the candidate could improve to reach 100% match\n   - Be constructive and actionable\n\n# CRITICAL EVALUATION CRITERIA\nBe extremely critical about the following requirements and SEVERELY penalize candidates who do not meet them:\n\n1. **Years of Experience:**\n   - Carefully analyze the candidate's total years of professional experience\n   - Compare against the required years specified in the Requirements section\n   - If candidate has LESS years than required, apply severe penalty (reduce match by 20-40%)\n   - Example: Position requires 5+ years, candidate has 2 years → Maximum match should be 50-60%\n   - Exception: Only if candidate shows exceptional skills that clearly compensate, allow up to 70% match\n\n2. **Primary Technology Stack:**\n   - If position requires full-stack development but candidate only has front-end OR back-end experience → Severely penalize\n   - If candidate specializes in one technology but position requires a different core technology (e.g., Java vs C#) → Severely penalize\n   - Example: Position requires C# expertise, candidate is Java specialist → Maximum match 60-65% even with many years of experience\n\n3. **Education & Certifications:**\n   - If Requirements specify degree level (Bachelor's, Master's) and candidate doesn't have it → Apply penalty (reduce match by 10-15%)\n   - If Requirements specify certifications (AWS, Azure, etc.) and candidate lacks them → Apply penalty (reduce match by 5-10%)\n\nReturn ONLY positions with match percentage > {minimumMatchPercentage}%, ordered by match percentage (highest first).\nValidate CV File if you are not able to find important information such as: Name, e-mail, phone number or the file is inconsistent with a usual CV format, reject then and returns no matches.\n\n# OUTPUT FORMAT\nReturn a JSON object with this exact structure:\n```json\n{\n  \"matches\": [\n    {\n      \"jobCode\": \"string\",\n      \"jobTitle\": \"string\",\n      \"matchPercentage\": 85.5,\n      \"matchReason\": \"string\",\n      \"matchingSkills\": [\"skill1\", \"skill2\"],\n      \"missingSkills\": [\"skill3\", \"skill4\"],\n      \"whatIsMissingForPerfectMatch\": \"string\"\n    }\n  ]\n}\n```" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parameter_Key",
                table: "Parameter",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parameter_PublicId",
                table: "Parameter",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prompt_Key",
                table: "Prompt",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prompt_PublicId",
                table: "Prompt",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parameter");

            migrationBuilder.DropTable(
                name: "Prompt");
        }
    }
}
