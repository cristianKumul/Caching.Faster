FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Caching.Faster.Proxy/Caching.Faster.Proxy.csproj", "Caching.Faster.Proxy/"]
RUN dotnet restore "Caching.Faster.Proxy/Caching.Faster.Proxy.csproj"
COPY . .
WORKDIR "/src/Caching.Faster.Proxy"
RUN dotnet build "Caching.Faster.Proxy.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Caching.Faster.Proxy.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Caching.Faster.Proxy.dll"]