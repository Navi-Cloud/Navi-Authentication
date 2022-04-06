FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NaviAuth/NaviAuth.csproj", "NaviAuth/"]
RUN dotnet restore "NaviAuth/NaviAuth.csproj"
COPY . .
WORKDIR "/src/NaviAuth"
RUN dotnet build "NaviAuth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NaviAuth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NaviAuth.dll"]
