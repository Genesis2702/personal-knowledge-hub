FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/Logs && chown "$APP_UID" /app/Logs
USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

COPY ["src/PersonalKnowledgeHub.Api/PersonalKnowledgeHub.csproj", "src/PersonalKnowledgeHub.Api/"]
RUN dotnet restore "src/PersonalKnowledgeHub.Api/PersonalKnowledgeHub.csproj"

COPY . .
WORKDIR "/repo/src/PersonalKnowledgeHub.Api"
RUN dotnet build "PersonalKnowledgeHub.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PersonalKnowledgeHub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalKnowledgeHub.dll"]