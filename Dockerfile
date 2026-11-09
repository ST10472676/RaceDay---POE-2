# Multi-stage build for the MVC front end. The API remains a separate service.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/RaceDay.Mvc/RaceDay.Mvc.csproj src/RaceDay.Mvc/
RUN dotnet restore src/RaceDay.Mvc/RaceDay.Mvc.csproj
COPY src/RaceDay.Mvc/ src/RaceDay.Mvc/
RUN dotnet publish src/RaceDay.Mvc/RaceDay.Mvc.csproj -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
