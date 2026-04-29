FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore SavePoint.Host/SavePoint.Host.csproj
RUN dotnet publish SavePoint.Host/SavePoint.Host.csproj -c Release -o /app/publish /p:DisableFrontendBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SavePoint.Host.dll"]