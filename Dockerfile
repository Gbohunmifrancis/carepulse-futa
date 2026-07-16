# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files first to restore dependencies
COPY src/FutaMedical.sln ./src/
COPY src/FutaMedical.API/FutaMedical.API.csproj ./src/FutaMedical.API/
COPY src/FutaMedical.Application/FutaMedical.Application.csproj ./src/FutaMedical.Application/
COPY src/FutaMedical.Domain/FutaMedical.Domain.csproj ./src/FutaMedical.Domain/
COPY src/FutaMedical.Infrastructure/FutaMedical.Infrastructure.csproj ./src/FutaMedical.Infrastructure/

RUN dotnet restore src/FutaMedical.sln

# Copy all source files
COPY src/ ./src/

# Build and publish API
RUN dotnet publish src/FutaMedical.API/FutaMedical.API.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FutaMedical.API.dll"]
