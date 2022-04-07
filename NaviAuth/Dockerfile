FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NaviAuth/NaviAuth.csproj", "NaviAuth/"]
RUN dotnet restore "NaviAuth/NaviAuth.csproj"
COPY . .
WORKDIR "/src/NaviAuth"
RUN dotnet publish "NaviAuth.csproj" -c Debug -o /app/publish
WORKDIR /app/publish
ENTRYPOINT ["dotnet", "NaviAuth.dll"]