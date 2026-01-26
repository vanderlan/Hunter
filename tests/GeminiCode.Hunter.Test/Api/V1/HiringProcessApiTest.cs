using System;
using System.Net;
using System.Threading.Tasks;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetPaginated;
using GeminiCode.Hunter.Application.Core.UseCases.HiringProcess.Queries.GetDashboard;
using GeminiCode.Hunter.Domain.Core.ValueObjects.Pagination;
using GeminiCode.Hunter.Test.Integration.Setup;
using GeminiCode.Hunter.Domain.Core.Entities.Enuns;
using Xunit;

namespace GeminiCode.Hunter.Test.Api.V1
{
    public class HiringProcessApiTest : IntegrationTestsBootstrapper
    {
        public HiringProcessApiTest(IntegrationTestsFixture fixture) : base(fixture)
        {
            LoginWithDefaultUser();
        }
        
        [Fact]
        public async Task GetHiringProcessPaginated_WithoutFilter_ReturnsHiringProcesses()
        {
            //act
            var getHiringProcessesHttpResponse = await AuthenticatedHttpClient.GetAsync("/HiringProcess");

            var listHiringProcessesPaginated = await GetResultContentAsync<PagedList<HiringProcessGetPaginatedResponse>>(getHiringProcessesHttpResponse);

            //assert
            Assert.True(listHiringProcessesPaginated.Count >= 0);
            Assert.Equal(HttpStatusCode.OK, getHiringProcessesHttpResponse.StatusCode);
        }
        
        [Fact]
        public async Task GetHiringProcessPaginated_WithFilter_ReturnsFilteredHiringProcesses()
        {
            //act
            var getHiringProcessesHttpResponse = await AuthenticatedHttpClient.GetAsync($"/HiringProcess?filter.Status={(int)HiringProcessStatus.ReferredByPartner}");

            var listHiringProcessesPaginated = await GetResultContentAsync<PagedList<HiringProcessGetPaginatedResponse>>(getHiringProcessesHttpResponse);

            //assert
            Assert.True(listHiringProcessesPaginated.Count >= 0);
            Assert.Equal(HttpStatusCode.OK, getHiringProcessesHttpResponse.StatusCode);
        }

        [Fact]
        public async Task GetHiringProcessById_WithInvalidId_ReturnsNotFound()
        {
            //arrange & act
            var hiringProcessGetHttpResponse = await AuthenticatedHttpClient.GetAsync($"/HiringProcess/{Guid.NewGuid()}");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, hiringProcessGetHttpResponse.StatusCode);
        }

        [Fact]
        public async Task GetDashboard_ReturnsValidDashboard()
        {
            //arrange & act
            var dashboardHttpResponse = await AuthenticatedHttpClient.GetAsync("/HiringProcess/Dashboard");

            var dashboard = await GetResultContentAsync<HiringProcessGetDashboardResponse>(dashboardHttpResponse);

            //assert
            Assert.NotNull(dashboard);
            Assert.Equal(HttpStatusCode.OK, dashboardHttpResponse.StatusCode);
        }
    }
}
