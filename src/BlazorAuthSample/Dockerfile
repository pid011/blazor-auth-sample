# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/BlazorAuthSample/BlazorAuthSample.csproj", "src/BlazorAuthSample/"]
COPY ["src/BlazorAuthSample.ServiceDefaults/BlazorAuthSample.ServiceDefaults.csproj", "src/BlazorAuthSample.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "src/BlazorAuthSample/BlazorAuthSample.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/BlazorAuthSample"
RUN dotnet build "BlazorAuthSample.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BlazorAuthSample.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BlazorAuthSample.dll"]
