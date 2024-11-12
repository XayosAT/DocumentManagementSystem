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

# Install Tesseract OCR and dependencies
# Install required libraries and Ghostscript
RUN apt-get update && \
    apt-get install -y tesseract-ocr libtesseract-dev ghostscript && \
    apt-get clean

# Copy the local tessdata folder into the container (assumes ./tessdata exists)
COPY ./DocumentWorkerService/tessdata /app/tessdata

# Set the working directory and copy built files
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8082 if needed
EXPOSE 8082

# Set the environment variable for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TESSDATA_PREFIX /app/tessdata

# Run the Worker Service
ENTRYPOINT ["dotnet", "DocumentWorkerService.dll"]
