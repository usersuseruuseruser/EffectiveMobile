FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EffectiveMobile.SecondService/EffectiveMobile.SecondService.csproj", "EffectiveMobile.SecondService/"]
RUN dotnet restore "EffectiveMobile.SecondService/EffectiveMobile.SecondService.csproj"
COPY . .
WORKDIR "/src/EffectiveMobile.SecondService"
RUN dotnet build "EffectiveMobile.SecondService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EffectiveMobile.SecondService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EffectiveMobile.SecondService.dll"]
