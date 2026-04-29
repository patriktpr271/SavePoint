FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore SavePoint.ReviewsService/SavePoint.ReviewsService.csproj
RUN dotnet publish SavePoint.ReviewsService/SavePoint.ReviewsService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SavePoint.ReviewsService.dll"]