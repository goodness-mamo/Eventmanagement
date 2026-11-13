

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EventManagementApi/EventManagementApi.csproj EventManagementApi/
RUN dotnet restore EventManagementApi/EventManagementApi.csproj

COPY . .
WORKDIR /src/EventManagementApi
RUN dotnet publish EventManagementApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EventManagementApi.dll"]

