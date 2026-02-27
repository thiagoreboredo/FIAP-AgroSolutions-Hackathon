FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/IngestionService/IngestionService.csproj", "src/IngestionService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/IngestionService/IngestionService.csproj"
COPY . .
WORKDIR "/src/src/IngestionService"
RUN dotnet build "IngestionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IngestionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IngestionService.dll"]
