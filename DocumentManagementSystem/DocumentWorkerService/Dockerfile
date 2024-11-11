# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["DocumentWorkerService/DocumentWorkerService.csproj", "DocumentWorkerService/"]
RUN dotnet restore "DocumentWorkerService/DocumentWorkerService.csproj"

# Copy the rest of the application code and build the app
COPY DocumentWorkerService/ DocumentWorkerService/
WORKDIR "/src/DocumentWorkerService"
RUN dotnet publish -c Release -o /app/publish

# Use the .NET Runtime image for the runtime environment
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8082 if needed
EXPOSE 8082

# Set the environment variables for the production environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the Worker Service
ENTRYPOINT ["dotnet", "DocumentWorkerService.dll"]
