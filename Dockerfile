FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . ./

RUN dotnet restore "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj"
RUN dotnet publish "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Install the New Relic agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Enable the New Relic agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=88c9223c371e67d2f21b07be5d622589FFFFNRAL \
NEW_RELIC_APP_NAME="Hunter"

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_Kestrel__Limits__RequestHeadersTimeout=00:05:00
ENV ASPNETCORE_Kestrel__Limits__KeepAliveTimeout=00:05:00
EXPOSE 80

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "GeminiCode.Hunter.Api.dll"]