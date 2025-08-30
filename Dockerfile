# Use the .NET 9 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000

# Use the .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["TicketingPlatform.API/TicketingPlatform.API.csproj", "TicketingPlatform.API/"]
COPY ["Modules.UserService/Modules.UserService.csproj", "Modules.UserService/"]
COPY ["Modules.EventService/Modules.EventService.csproj", "Modules.EventService/"]
COPY ["Modules.TeamService/Modules.TeamService.csproj", "Modules.TeamService/"]
COPY ["Modules.TicketService/Modules.TicketService.csproj", "Modules.TicketService/"]
COPY ["Modules.PaymentService/Modules.PaymentService.csproj", "Modules.PaymentService/"]
COPY ["Shared.Kernel/Shared.Kernel.csproj", "Shared.Kernel/"]

# Restore dependencies
RUN dotnet restore "TicketingPlatform.API/TicketingPlatform.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/TicketingPlatform.API"
RUN dotnet build "TicketingPlatform.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TicketingPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "TicketingPlatform.API.dll"]
