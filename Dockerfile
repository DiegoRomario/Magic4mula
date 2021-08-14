#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/M4.WebApi/M4.WebApi.csproj", "src/M4.WebApi/"]
COPY ["src/M4.Infrastructure/M4.Infrastructure.csproj", "src/M4.Infrastructure/"]
COPY ["src/M4.Domain/M4.Domain.csproj", "src/M4.Domain/"]
RUN dotnet restore "src/M4.WebApi/M4.WebApi.csproj"
RUN dotnet restore "src/M4.Infrastructure/M4.Infrastructure.csproj"
RUN dotnet restore "src/M4.Domain/M4.Domain.csproj"
RUN add-migrations initial -Context "src/M4.Domain/M4.Domain.cspro
COPY . .
WORKDIR "/src/src/M4.WebApi"
RUN dotnet build "M4.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "M4.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "M4.WebApi.dll"]