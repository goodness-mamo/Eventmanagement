

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EventManagement.Mvc/EventManagement.Mvc.csproj EventManagement.Mvc/
RUN dotnet restore EventManagement.Mvc/EventManagement.Mvc.csproj

COPY . .
WORKDIR /src/EventManagement.Mvc
RUN dotnet publish EventManagement.Mvc.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EventManagement.Mvc.dll"]

