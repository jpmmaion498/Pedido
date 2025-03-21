#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pedido.Api/Pedido.Api.csproj", "Pedido.Api/"]
COPY ["Pedido.Application/Pedido.Application.csproj", "Pedido.Application/"]
COPY ["Pedido.Domain/Pedido.Domain.csproj", "Pedido.Domain/"]
COPY ["Pedido.Infrastructure/Pedido.Infrastructure.csproj", "Pedido.Infrastructure/"]
RUN dotnet restore "./Pedido.Api/Pedido.Api.csproj"
COPY . .
WORKDIR "/src/Pedido.Api"
RUN dotnet build "./Pedido.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pedido.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pedido.Api.dll"]