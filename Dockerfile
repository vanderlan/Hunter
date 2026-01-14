FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . ./

RUN dotnet restore "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj"
RUN dotnet publish "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Enable the New Relic agent (using NuGet package included in the build)
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/app/newrelic \
CORECLR_PROFILER_PATH=/app/newrelic/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=88c9223c371e67d2f21b07be5d622589FFFFNRAL \
NEW_RELIC_APP_NAME=Hunter

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_Kestrel__Limits__RequestHeadersTimeout=00:05:00
ENV ASPNETCORE_Kestrel__Limits__KeepAliveTimeout=00:05:00
EXPOSE 80

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "GeminiCode.Hunter.Api.dll"]