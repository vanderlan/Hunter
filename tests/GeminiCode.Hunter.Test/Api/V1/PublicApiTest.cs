using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.Public.Queries.GetPublicJobPositions;
using GeminiCode.Hunter.Test.Integration.Setup;
using Xunit;

namespace GeminiCode.Hunter.Test.Api.V1
{
    public class PublicApiTest : IntegrationTestsBootstrapper
    {
        public PublicApiTest(IntegrationTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task GetPublicJobPositions_WithoutToken_ReturnsOk()
        {
            var response = await HttpClient.GetAsync("/Public/JobPositions");

            var body = await GetResultContentAsync<GetPublicJobPositionsResponse>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(body.Jobs.Count, body.TotalCount);
        }

        [Fact]
        public async Task GetPublicJobPosition_WithUnknownCode_ReturnsNotFound()
        {
            var response = await HttpClient.GetAsync($"/Public/JobPositions/{Guid.NewGuid():N}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithUnknownToken_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync($"/Public/Candidates/{Guid.NewGuid():N}/Register", new
            {
                name = "Jane Doe",
                email = "jane@doe.com",
                hasAcceptedTerms = true
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithoutAcceptingTerms_ReturnsBadRequest()
        {
            var response = await HttpClient.PostAsJsonAsync($"/Public/Candidates/{Guid.NewGuid():N}/Register", new
            {
                name = "Jane Doe",
                email = "jane@doe.com",
                hasAcceptedTerms = false
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ConfirmEmail_WithUnknownToken_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("/Public/Candidates/Confirm", new { token = Guid.NewGuid().ToString("N") });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CvMatch_WithoutFile_ReturnsBadRequest()
        {
            using var content = new MultipartFormDataContent();

            var response = await HttpClient.PostAsync("/Public/CvMatch", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ReferralRequests_WithoutToken_ReturnsUnauthorized()
        {
            var response = await HttpClient.GetAsync("/ReferralRequests");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
