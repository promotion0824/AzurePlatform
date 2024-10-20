FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Willow.OpsBot.Common/Willow.OpsBot.Common.csproj", "Willow.OpsBot.Common/"]
RUN dotnet restore "Willow.OpsBot.Common/Willow.OpsBot.Common.csproj"
COPY ["Willow.OpsBot.Api/Willow.OpsBot.Api.csproj", "Willow.OpsBot.Api/"]
RUN dotnet restore "Willow.OpsBot.Api/Willow.OpsBot.Api.csproj"
COPY . .
WORKDIR "/src/Willow.OpsBot.Api"
ARG Version="1.0.0.0"
RUN dotnet build "Willow.OpsBot.Api.csproj" -c Release /p:Version=$Version

FROM build AS publish
RUN dotnet publish "Willow.OpsBot.Api.csproj" -c Release -o /app/publish  --no-restore --no-build

FROM base AS final
WORKDIR /app
RUN chown 1000:1000 /app
USER 1000
ENV ASPNETCORE_URLS http://*:8080
COPY --chown=1000:1000 --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.OpsBot.Api.dll"]
