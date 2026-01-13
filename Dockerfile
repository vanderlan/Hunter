FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY . ./

RUN dotnet restore "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj"
RUN dotnet publish "src/GeminiCode.Hunter.Api/GeminiCode.Hunter.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_Kestrel__Limits__RequestHeadersTimeout=00:05:00
ENV ASPNETCORE_Kestrel__Limits__KeepAliveTimeout=00:05:00
EXPOSE 80

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "GeminiCode.Hunter.Api.dll"]