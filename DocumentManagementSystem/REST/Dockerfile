# Stage 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["REST/REST.csproj", "REST/"]

# Restore dependencies
RUN dotnet restore "REST/REST.csproj"

# Copy the rest of the source code
COPY . .

# Build the projects (ensure that the DAL builds before the REST)
RUN dotnet build "DAL/DAL.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet build "REST/REST.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "REST/REST.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "REST.dll"]
