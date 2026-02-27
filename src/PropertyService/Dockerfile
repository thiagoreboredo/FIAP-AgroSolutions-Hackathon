FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PropertyService/PropertyService.csproj", "src/PropertyService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/PropertyService/PropertyService.csproj"
COPY . .
WORKDIR "/src/src/PropertyService"
RUN dotnet build "PropertyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PropertyService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PropertyService.dll"]
