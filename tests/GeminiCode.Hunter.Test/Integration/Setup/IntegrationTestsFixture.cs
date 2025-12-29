using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeminiCode.Hunter.Domain.Core.Entities;
using GeminiCode.Hunter.Domain.Core.Repositories.UnitOfWork;
using System;
using System.Net.Http;
using GeminiCode.Hunter.Test.Shared.Faker;
using Npgsql;

namespace GeminiCode.Hunter.Test.Integration.Setup
{
    public class IntegrationTestsFixture
    {
        public readonly HttpClient HttpClient;
        public readonly HttpClient AuthenticatedHttpClient;
        public readonly User DefaultSystemUser;
        public readonly IServiceProvider ServiceProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly WebApplicationFactory<Program> _appFactory;
        private string _databaseName;
        private string _connectionString;

        public IntegrationTestsFixture()
        {
            var appFactory = new WebApplicationFactory<Program>()
                                .WithWebHostBuilder(builder =>
                                {
                                    var config = new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
                                        .Build();

                                    SetupPostgreSqlDatabase(config);

                                    builder.UseConfiguration(config);
                                });

            ServiceProvider = appFactory.Services;

            _unitOfWork = appFactory.Services.GetRequiredService<IUnitOfWork>();

            HttpClient = appFactory.CreateClient();
            AuthenticatedHttpClient = appFactory.CreateClient();

            DefaultSystemUser = UserFaker.GetDefaultSystemUser();

            _appFactory = appFactory;

            _unitOfWork.UserRepository.AddAsync(DefaultSystemUser).GetAwaiter().GetResult();
            _unitOfWork.CommitAsync().GetAwaiter().GetResult();
        }

        private void SetupPostgreSqlDatabase(IConfiguration config)
        {
            const string connectionPath = "ConnectionStrings:PostgreSql";

            _databaseName = $"HunterTests{DateTime.UtcNow.Ticks}";

            var postgreSqlConnection = new NpgsqlConnection(config[connectionPath]);

            lock (postgreSqlConnection)
            {
                using (postgreSqlConnection)
                {
                    postgreSqlConnection.Open();

                    var command = new NpgsqlCommand($"CREATE DATABASE {_databaseName}", postgreSqlConnection);

                    command.ExecuteNonQuery();
                    postgreSqlConnection.Close();
                }
            }

            _connectionString = config[connectionPath].Replace("HunterTests", _databaseName);
            config[connectionPath] = _connectionString;
        }

        public HttpClient GetNewHttpClient()
        {
            return _appFactory.CreateClient();
        }
    }
}
