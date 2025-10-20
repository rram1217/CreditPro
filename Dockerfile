# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["CreditPro/CreditPro.csproj", "CreditPro/"]

# Restore dependencies
RUN dotnet restore "CreditPro/CreditPro.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/CreditPro"
RUN dotnet build "CreditPro.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CreditPro.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CreditPro.dll"]

---