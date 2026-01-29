using System;
using System.Net;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.Candidates.Queries.GetPaginated;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Test.Integration.Setup;
using Xunit;

namespace GeminiCode.Hunter.Test.Api.V1
{
    public class CandidatesApiTest : IntegrationTestsBootstrapper
    {
        public CandidatesApiTest(IntegrationTestsFixture fixture) : base(fixture)
        {
            LoginWithDefaultUser();
        }
        
        [Fact]
        public async Task GetCandidatePaginated_WithoutFilter_ReturnsCandidates()
        {
            //act
            var getCandidatesHttpResponse = await AuthenticatedHttpClient.GetAsync("/Candidates");

            var listCandidatesPaginated = await GetResultContentAsync<PagedList<CandidateGetPaginatedResponse>>(getCandidatesHttpResponse);

            //assert
            Assert.True(listCandidatesPaginated.Count >= 0);
            Assert.Equal(HttpStatusCode.OK, getCandidatesHttpResponse.StatusCode);
        }
        
        [Fact]
        public async Task GetCandidatePaginated_WithInvalidName_ReturnsEmptyList()
        {
            //arrange & act
            var getCandidatesHttpResponse = await AuthenticatedHttpClient.GetAsync($"/Candidates?filter.Query={Guid.NewGuid()}");

            var listCandidatesPaginated = await GetResultContentAsync<PagedList<CandidateGetPaginatedResponse>>(getCandidatesHttpResponse);

            //assert
            Assert.Empty(listCandidatesPaginated.Items);
            Assert.Equal(HttpStatusCode.OK, getCandidatesHttpResponse.StatusCode);
        }

        [Fact]
        public async Task GetCandidateById_WithInvalidId_ReturnsNotFound()
        {
            //arrange & act
            var candidateGetHttpResponse = await AuthenticatedHttpClient.GetAsync($"/Candidates/{Guid.NewGuid()}");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, candidateGetHttpResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCandidate_WithInvalidId_ReturnsNotFound()
        {
            //arrange & act
            var httpResponseDelete = await AuthenticatedHttpClient.DeleteAsync($"/Candidates/{Guid.NewGuid()}");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, httpResponseDelete.StatusCode);
        }
    }
}
