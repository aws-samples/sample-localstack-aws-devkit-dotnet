# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine@sha256:1659a7a350b7847f6afc84df1409c6b80fd08e7decf51f5c365303b0d625f13b AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only necessary files for dependency restoration
COPY ./app/OrderManagementSystem.API/OrderManagementSystem.API.csproj OrderManagementSystem.API/
COPY ./app/OrderManagementSystem.Common/OrderManagementSystem.Common.csproj OrderManagementSystem.Common/
COPY ./app/OrderManagementSystem.Configurations/OrderManagementSystem.Configurations.csproj OrderManagementSystem.Configurations/
COPY ./app/OrderManagementSystem.Data/OrderManagementSystem.Data.csproj OrderManagementSystem.Data/
COPY ./app/OrderManagementSystem.Services/OrderManagementSystem.Services.csproj OrderManagementSystem.Services/
RUN dotnet restore "OrderManagementSystem.API/OrderManagementSystem.API.csproj"

# Copy the source code
COPY ./app ./

# Build and publish in a single stage
WORKDIR "/src/OrderManagementSystem.API"
RUN dotnet publish "OrderManagementSystem.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create user-group and user. Grant permission to user on app folder
RUN addgroup -S appgroup && adduser -S appuser -G appgroup && chown -R appuser:appgroup /app

# Final runtime stage using chiseled container
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled@sha256:09d88164bedd70b05f12ecd463ddc7c9a5ebe12e4c2bb2bf50ce224049738d9f AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Copy user-related config and set active user
COPY --from=build /etc/passwd /etc/passwd
COPY --from=build /home/appuser /home/appuser
USER appuser

# Copy app binaries
COPY --from=build /app/publish /app

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD ["wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:8080/health", "||", "exit", "1"]

ENTRYPOINT ["dotnet", "OrderManagementSystem.API.dll"]