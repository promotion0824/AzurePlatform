FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Willow.OpsBot.Common/Willow.OpsBot.Common.csproj", "Willow.OpsBot.Common/"]
RUN dotnet restore "Willow.OpsBot.Common/Willow.OpsBot.Common.csproj"
COPY ["Willow.OpsBot.WorkerService/Willow.OpsBot.WorkerService.csproj", "Willow.OpsBot.WorkerService/"]
RUN dotnet restore "Willow.OpsBot.WorkerService/Willow.OpsBot.WorkerService.csproj"
COPY . .
WORKDIR "/src/Willow.OpsBot.WorkerService"
ARG Version="1.0.0.0"
RUN dotnet build "Willow.OpsBot.WorkerService.csproj" -c Release  /p:Version=$Version

FROM build AS publish
RUN dotnet publish "Willow.OpsBot.WorkerService.csproj" -c Release -o /app/publish  --no-restore --no-build

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Willow.OpsBot.WorkerService.dll"]
