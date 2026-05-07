using System;
using System.Net;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Commands.Create;
using GeminiCode.Hunter.Application.Core.UseCases.JobPositions.Queries.GetPaginated;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Test.Integration.Setup;
using GeminiCode.Hunter.Test.Shared.Faker;
using Xunit;

namespace GeminiCode.Hunter.Test.Api.V1
{
    public class JobPositionsApiTest : IntegrationTestsBootstrapper
    {
        public JobPositionsApiTest(IntegrationTestsFixture fixture) : base(fixture)
        {
            LoginWithDefaultUser();
        }
        
        [Fact]
        public async Task GetJobPositionPaginated_WithoutFilter_ReturnsJobPositions()
        {
            //arrange
            var createdJobPosition = await CreateJobPositionAsync();
            
            //act
            var getJobPositionsHttpResponse = await AuthenticatedHttpClient.GetAsync("/JobPositions");

            var listJobPositionsPaginated = await GetResultContentAsync<PagedList<JobPositionGetPaginatedResponse>>(getJobPositionsHttpResponse);

            //assert
            Assert.Contains(listJobPositionsPaginated.Items, x => x.PublicId == createdJobPosition.PublicId);
            Assert.Contains(listJobPositionsPaginated.Items, x => x.Title == createdJobPosition.Title);
            Assert.True(listJobPositionsPaginated.Count >= 1);
            Assert.Equal(HttpStatusCode.OK, getJobPositionsHttpResponse.StatusCode);
        }
        
        [Fact]
        public async Task GetJobPositionPaginated_WithTitleFilter_ReturnsFilteredJobPositions()
        {
            //arrange
            var createdJobPosition = await CreateJobPositionAsync();
            
            //act
            var getJobPositionsHttpResponse = await AuthenticatedHttpClient.GetAsync($"/JobPositions?filter.Query={createdJobPosition.Title}");

            var listJobPositionsPaginated = await GetResultContentAsync<PagedList<JobPositionGetPaginatedResponse>>(getJobPositionsHttpResponse);

            //assert
            Assert.Contains(listJobPositionsPaginated.Items, x => x.PublicId == createdJobPosition.PublicId);
            Assert.Contains(listJobPositionsPaginated.Items, x => x.Title == createdJobPosition.Title);
            Assert.True(listJobPositionsPaginated.Count >= 1);
            Assert.Equal(HttpStatusCode.OK, getJobPositionsHttpResponse.StatusCode);
        }
        
        [Fact]
        public async Task GetJobPositionPaginated_WithInvalidTitle_ReturnsEmptyList()
        {
            //arrange & act
            var getJobPositionsHttpResponse = await AuthenticatedHttpClient.GetAsync($"/JobPositions?filter.Query={Guid.NewGuid()}");

            var listJobPositionsPaginated = await GetResultContentAsync<PagedList<JobPositionGetPaginatedResponse>>(getJobPositionsHttpResponse);

            //assert
            Assert.Empty(listJobPositionsPaginated.Items);
            Assert.Equal(HttpStatusCode.OK, getJobPositionsHttpResponse.StatusCode);
        }

        [Fact]
        public async Task PostJobPosition_WithValidData_CreateAJobPosition()
        {
            //arrange
            var request = JobPositionFaker.GetJobPositionCreateRequest();

            //act
            var httpResponse = await AuthenticatedHttpClient.PostAsync("/JobPositions", GetStringContent(request));

            var jobPositionCreated = await GetResultContentAsync<JobPositionCreateResponse>(httpResponse);

            //assert
            Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

            Assert.NotNull(jobPositionCreated);
            Assert.Equal(request.Title, jobPositionCreated.Title);
            Assert.True(jobPositionCreated.CreatedAt > DateTime.MinValue);
            Assert.True(jobPositionCreated.LastUpdated > DateTime.MinValue);
            Assert.NotNull(jobPositionCreated.PublicId);
        }

        [Fact]
        public async Task PutJobPosition_WithValidData_UpdateJobPosition()
        {
            //arrange
            var jobPositionCreated = await CreateJobPositionAsync();

            var jobPositionUpdateRequest = JobPositionFaker.GetJobPositionUpdateRequest();
            jobPositionUpdateRequest.PublicId = jobPositionCreated.PublicId;

            //act
            var httpResponsePut = await AuthenticatedHttpClient.PutAsync($"/JobPositions/{jobPositionCreated.PublicId}", GetStringContent(jobPositionUpdateRequest));

            Assert.Equal(HttpStatusCode.Accepted, httpResponsePut.StatusCode);

            var jobPositionGetHttpResponse = await AuthenticatedHttpClient.GetAsync($"/JobPositions/{jobPositionCreated.PublicId}");

            var jobPosition = await GetResultContentAsync<JobPositionCreateResponse>(jobPositionGetHttpResponse);

            //assert
            Assert.NotNull(jobPosition);
            Assert.Equal(jobPositionUpdateRequest.Title, jobPosition.Title);
            Assert.NotNull(jobPosition.PublicId);
        }

        [Fact]
        public async Task DeleteJobPosition_WithValidIdData_DeleteTheJobPosition()
        {
            //arrange
            var jobPositionCreated = await CreateJobPositionAsync();

            //act
            var httpResponseDelete = await AuthenticatedHttpClient.DeleteAsync($"/JobPositions/{jobPositionCreated.PublicId}");

            Assert.Equal(HttpStatusCode.NoContent, httpResponseDelete.StatusCode);

            var jobPositionGetHttpResponse = await AuthenticatedHttpClient.GetAsync($"/JobPositions/{jobPositionCreated.PublicId}");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, jobPositionGetHttpResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteJobPosition_WithInvalidId_ReturnsNotFound()
        {
            //arrange & act
            var httpResponseDelete = await AuthenticatedHttpClient.DeleteAsync($"/JobPositions/{Guid.NewGuid()}");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, httpResponseDelete.StatusCode);
        }
        
        private async Task<JobPositionCreateResponse> CreateJobPositionAsync(JobPositionCreateRequest jobPositionCreateRequest = null)
        {
            jobPositionCreateRequest ??= JobPositionFaker.GetJobPositionCreateRequest();

            var httpResponsePost = await AuthenticatedHttpClient.PostAsync("/JobPositions", GetStringContent(jobPositionCreateRequest));

            Assert.Equal(HttpStatusCode.Created, httpResponsePost.StatusCode);

            return await GetResultContentAsync<JobPositionCreateResponse>(httpResponsePost);
        }
    }
}
