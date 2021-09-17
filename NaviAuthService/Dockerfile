FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NaviAuthService/NaviAuthService.csproj", "NaviAuthService/"]
RUN dotnet restore "NaviAuthService/NaviAuthService.csproj"
COPY . .
WORKDIR "/src/NaviAuthService"
RUN dotnet build "NaviAuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NaviAuthService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NaviAuthService.dll"]
